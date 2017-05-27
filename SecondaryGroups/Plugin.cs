using System;
using System.Diagnostics;
using System.Linq;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace SecondaryGroups
{
  [ApiVersion(2, 1)]
  public class Plugin : TerrariaPlugin
  {
    public override string Name => "SecondaryGroups";
    public override string Author => "Newy";
    public override string Description => "XenForo style secondary groups!";
    public override Version Version => typeof(Plugin).Assembly.GetName().Version;

    public Plugin(Main game) : base(game)
    {
    }

    public override void Initialize()
    {
      Database.Connect();
      PlayerHooks.PlayerPermission += OnPlayerPermission;

      TShockAPI.Commands.ChatCommands.Add(
        new Command("secondarygroups.modify", CommandRouter, "sgroup", "secgroup")
      );
    }

    private static void CommandRouter(CommandArgs args)
    {
      if (args.Parameters.Count < 2)
      {
        args.Player.SendErrorMessage("Invalid usage! Usage: /sgroup <add/delete/list> <user> <group>");
        return;
      }

      var input = args.Parameters[0].ToLowerInvariant();
      args.Parameters.RemoveAt(0);

      try
      {
        switch (input)
        {
          case "add":
            Commands.AddCommand(args);
            break;

          case "del":
          case "delete":
            Commands.RemoveCommand(args);
            break;

          case "list":
            Commands.ListCommand(args);
            break;

          default:
            args.Player.SendErrorMessage("Invalid usage! Usage: /sgroup <add/delete/list> <user> <group>");
            return;
        }
      }
      catch (Exception e)
      {
        args.Player.SendErrorMessage(e.Message);
      }
    }

    private static void OnPlayerPermission(PlayerPermissionEventArgs e)
    {
#if DEBUG
      var sv = Stopwatch.StartNew();
#endif
      if (e.Player.User == null) return;

      var data = GroupData.Get(e.Player.User);

      if (data == null) return;

      if (data.Permissions.Contains(e.Permission))
        e.Handled = true;

#if DEBUG
      sv.Stop();
      Console.WriteLine("Permission check spent:" + sv.ElapsedMilliseconds);
#endif
    }

    protected override void Dispose(bool disposing)
    {
      PlayerHooks.PlayerPermission -= OnPlayerPermission;
      base.Dispose(disposing);
    }
  }
}