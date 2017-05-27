using System;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using TShockAPI;
using TShockAPI.DB;

namespace SecondaryGroups
{
  internal static class Database
  {
    internal static IDbConnection Connection { get; private set; }

    private static readonly SqlTable TableStructure =
      new SqlTable("SecondaryGroups",
        new SqlColumn("ID", MySqlDbType.Int32) {NotNull = true, Primary = true, Unique = true},
        new SqlColumn("Groups", MySqlDbType.Text)
      );

    internal static void Connect()
    {
      switch (TShock.Config.StorageType.ToLowerInvariant())
      {
        case "sqlite":
          Connection = new SqliteConnection(
            "uri=file://" + Path.Combine(TShock.SavePath, "SecondaryGroups.sqlite") + ",Version=3"
          );
          break;

        case "mysql":
          var host = TShock.Config.MySqlHost.Split(':');

          Connection = new MySqlConnection(
            string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
              host[0], host.Length == 1 ? "3306" : host[1],
              TShock.Config.MySqlDbName,
              TShock.Config.MySqlUsername,
              TShock.Config.MySqlPassword
            )
          );
          break;

        default:
          throw new InvalidOperationException(
            "Storage type " + TShock.Config.StorageType.ToLowerInvariant() + "is not supported."
          );
      }

      var sqlcreator = new SqlTableCreator(
        Connection,
        Connection.GetSqlType() == SqlType.Sqlite
          ? (IQueryBuilder) new SqliteQueryCreator()
          : new MysqlQueryCreator()
      );

      sqlcreator.EnsureTableStructure(TableStructure);
    }
  }
}