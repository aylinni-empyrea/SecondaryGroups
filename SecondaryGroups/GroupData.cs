using System;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;
using TShockAPI.DB;

namespace SecondaryGroups
{
  public class GroupData
  {
    private static Dictionary<int, GroupData> _cache;
    public int ID { get; }

    public User User => TShock.Users.GetUserByID(ID);

    private List<Group> _groups;
    public IReadOnlyCollection<Group> Groups => _groups;

    //private string[] _permissions;
    //public IEnumerable<string> Permissions => _permissions ?? (_permissions = GetPermissions().ToArray());

    public IEnumerable<string> Permissions => GetPermissions();

    private Group GetTempgroup()
      => TShock.Players.FirstOrDefault(p => p?.tempGroup != null && p.User != null && p.User == User)?.tempGroup;

    private IEnumerable<string> GetPermissions()
      => _groups.SelectMany(grp => grp.TotalPermissions)
        .Concat(TShock.Groups.GetGroupByName(GetTempgroup()?.Name ?? User.Group).TotalPermissions)
        .Distinct();

    private static IEnumerable<GroupData> GetGroups()
    {
      using (var q = Database.Connection.QueryReader(@"SELECT * FROM SecondaryGroups;"))
      {
        while (q.Read())
          yield return new GroupData(q.Get<int>("ID"), q.Get<string>("Groups"));
      }
    }

    private GroupData(int userId, string secondaryGroups)
    {
      ID = userId;
      _groups = new List<Group>(secondaryGroups.Split(';')
        .Select(TShock.Groups.GetGroupByName).Where(g => g != null));
    }

    public static GroupData Create(User user, IEnumerable<string> groups)
    {
      var targets = groups.Select(TShock.Groups.GetGroupByName);

      if (targets.Any(t => t == null))
        throw new Exception("Invalid groups!");

      if (Database.Connection.Query(@"INSERT INTO SecondaryGroups VALUES (@0, @1);",
            user.ID, string.Join(";", groups)) != 1)
        throw new Exception("Unexpected error while saving new data to database.");

      return new GroupData(user.ID, string.Join(";", groups));
    }

    public static GroupData Get(User user)
    {
      if (_cache == null) _cache = GetGroups().ToDictionary(k => k.ID);

      if (_cache.TryGetValue(user.ID, out GroupData data))
        return data;

      using (var q = Database.Connection.QueryReader(@"SELECT * FROM SecondaryGroups WHERE ID = @0;", user.ID))
      {
        while (q.Read())
        {
          var ret = new GroupData(q.Get<int>("ID"), q.Get<string>("Groups"));
          _cache.Add(ret.ID, ret);
          return ret;
        }
      }

      return null;
    }

    public void AddGroups(IEnumerable<Group> groups)
    {
      groups = groups.ToArray();

      if (groups.Any(g => g == null))
        throw new ArgumentException("One of the names provided are invalid.");

      if (groups.Any(g => g.Name.Equals("superadmin")))
        throw new ArgumentException("Adding superadmin this way isn't supported.");

      var delta = groups.Join(groups, o => o.Name, i => i.Name, (o, i) => o, StringComparer.OrdinalIgnoreCase)
        .ToArray();

      if (delta.Length < 1)
        return;

      _groups.AddRange(delta);

      ResetCache();
      Save();
    }

    public void RemoveGroups(IEnumerable<Group> groups)
    {
      groups = groups.ToArray();

      if (groups.Any(g => g == null))
        throw new ArgumentException("One of the names provided are invalid.");

      _groups.RemoveAll(groups.Contains);

      ResetCache();
      Save();
    }

    public void Save()
    {
      if (Database.Connection.Query(@"UPDATE SecondaryGroups SET Groups = @1 WHERE ID = @0;",
            ID, string.Join(";", _groups)) != 1)
        throw new Exception("Unexpected error while saving to database.");
    }

    public static void ResetCache() => _cache = null;
  }
}