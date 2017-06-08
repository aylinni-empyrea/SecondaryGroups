using System.Linq;
using TShockAPI;

namespace SecondaryGroups
{
  internal static class Commands
  {
    internal static void ListCommand(CommandArgs args)
    {
      var users = TShock.Users.GetUsersByName(args.Parameters[0]);
      args.Parameters.RemoveAt(0);

      if (users.Count == 0)
      {
        args.Player.SendErrorMessage("No users matched!");
        return;
      }

      if (users.Count > 1)
      {
        TShock.Utils.SendMultipleMatchError(args.Player, users.Select(u => u.Name));
        return;
      }

      var user = users[0];
      var groupdata = GroupData.Get(user);

      if (groupdata == null || groupdata.Groups.Count == 0)
      {
        args.Player.SendErrorMessage("User has no secondary groups!");
        return;
      }

      args.Player.SendInfoMessage("Secondary groups of {0}:\n{1}", user.Name, string.Join(", ", groupdata.Groups));
    }

    internal static void RemoveCommand(CommandArgs args)
    {
      var users = TShock.Users.GetUsersByName(args.Parameters[0]);
      args.Parameters.RemoveAt(0);

      if (users.Count == 0)
      {
        args.Player.SendErrorMessage("No users matched!");
        return;
      }

      if (users.Count > 1)
      {
        TShock.Utils.SendMultipleMatchError(args.Player, users.Select(u => u.Name));
        return;
      }

      var user = users[0];
      var groupdata = GroupData.Get(user);

      if (groupdata == null || groupdata.Groups.Count == 0)
      {
        args.Player.SendErrorMessage("User has no secondary groups!");
        return;
      }

      if (args.Parameters.Count < 1 || args.Parameters.Any(string.IsNullOrWhiteSpace))
      {
        args.Player.SendErrorMessage("Invalid usage! Usage: /sgroup del [player] [groups]");
        return;
      }

      var foundGroups = args.Parameters.Select(TShock.Groups.GetGroupByName)
        .Where(g => groupdata.Groups.Contains(g)).ToArray();

      if (foundGroups.Length == 0)
      {
        args.Player.SendErrorMessage("Player isn't a member of those groups!");
        return;
      }

      groupdata.RemoveGroups(foundGroups);

      args.Player.SendSuccessMessage(
        "Secondary groups \"{0}\" removed from user {1} successfully!", string.Join(", ", args.Parameters), user.Name
      );
    }

    internal static void AddCommand(CommandArgs args)
    {
      var users = TShock.Users.GetUsersByName(args.Parameters[0]);
      args.Parameters.RemoveAt(0);

      if (users.Count == 0)
      {
        args.Player.SendErrorMessage("No users matched!");
        return;
      }

      if (users.Count > 1)
      {
        TShock.Utils.SendMultipleMatchError(args.Player, users.Select(u => u.Name));
        return;
      }

      var user = users[0];

      var groupdata = GroupData.Get(user);

      if (groupdata == null)
      {
        GroupData.Create(user, args.Parameters);
        return;
      }

      if (args.Parameters.Count < 1 || args.Parameters.Any(string.IsNullOrWhiteSpace))
      {
        args.Player.SendErrorMessage("Invalid groups!");
        return;
      }

      var providedGroups = args.Parameters.Select(TShock.Groups.GetGroupByName)
        .Where(g => !groupdata.Groups.Contains(g)).ToArray();

      if (providedGroups.Length == 0)
      {
        args.Player.SendErrorMessage("Player is already in all the provided groups!");
        return;
      }

      groupdata.AddGroups(providedGroups);

      args.Player.SendSuccessMessage(
        "Secondary groups \"{0}\" added to user {1} successfully!",
        string.Join(", ", providedGroups.Select(g => g.Name)), user.Name
      );
    }
  }
}