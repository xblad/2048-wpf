using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Common;
using System;

using Game2048.DataModels;

namespace Game2048.DataObjects
{
    class ResultDO
    {
        public Guid? GamerGuid { get; set; }
        public string GamerName { get; set; }
        public long GamerScores { get; set; }
        public long GamerBiggestTile { get; set; }

        public async static Task<bool> TestConnectionAsync() // asynchronous test of connection
        {
            using (Game2048Entities db = new Game2048Entities())
            {
                DbConnection conn = db.Database.Connection;
                try
                {
                    await conn.OpenAsync();   // check the database connection
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        
        public async static Task<List<ResultDO>> GetBestResultsAsync(int count = 5) // returns task to get list of results for statistics page
        {
            using (Game2048Entities context = new Game2048Entities())
            {
                return await context.vwgmrResults
                    .OrderByDescending(x => x.gmrResultGamerScore) // better scores first
                    .ThenBy(x => x.gmrResultDateTime) // older results first
                    .Select(x => new ResultDO()
                {
                    GamerGuid = x.gmrGamerId,
                    GamerName = x.gmrGamerName,
                    GamerScores = x.gmrResultGamerScore,
                    GamerBiggestTile = x.gmrResultGamerBiggestTile
                })
                .Take(count) // count of returned results
                .ToListAsync();
            }
        }

        public async static void SendResultToDatabaseAsync(Guid gamerGuid, long gamerScores, int gamerBiggestTile) // save progress to database
        {
            using (Game2048Entities context = new Game2048Entities())
            {
                // Create a new Result object.
                gmrResult res = new gmrResult
                {
                    gmrGamerId = gamerGuid,
                    gmrResultGamerScore = gamerScores,
                    gmrResultGamerBiggestTile = gamerBiggestTile,
                    gmrResultDateTime = DateTime.Now
                };

                // Add the new object to the Result collection.
                context.gmrResults.Add(res);
                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Debug.Print(e.InnerException.ToString());
                }
                
            }
        }

        public async static Task<bool> IsGuidInDatabaseAsync(Guid gamerGuid) // check if gamer already exists (asynchronous)
        {
            using (Game2048Entities context = new Game2048Entities())
            {
                try { return await context.gmrGamers.AnyAsync(x => x.gmrGamerId == gamerGuid); }
                catch { return false; }
            }
        }

        public static bool IsGuidInDatabase(Guid gamerGuid) // check if gamer already exists (synchronous)
        {
            using (Game2048Entities context = new Game2048Entities())
            {
                try { return context.gmrGamers.Any(x => x.gmrGamerId == gamerGuid); }
                catch { return false; }
            }
        }

        public static void SendGamerToDatabase(Guid gamerGuid, string gamerName) // create new gamer in database
        {
            using (Game2048Entities context = new Game2048Entities())
            {
                // Create a new Gamer object.
                gmrGamer gmr = new gmrGamer
                {
                    gmrGamerId = gamerGuid,
                    gmrGamerName = gamerName
                };

                // Add the new object to the Gamer collection.
                context.gmrGamers.Add(gmr);
                try
                {
                    context.SaveChanges();
                }
                catch (Exception e)
                {
                    Debug.Print(e.InnerException.ToString());
                }

            }
        }

        internal async static Task<string> GetGamerNameByGuidAsync(Guid gamerGuid) // returns task to get gamer name by Guid
        {
            using (Game2048Entities context = new Game2048Entities())
            {
                gmrGamer gamer = await context.gmrGamers.SingleOrDefaultAsync(x => x.gmrGamerId == gamerGuid);
                return gamer.gmrGamerName;
            }
        }
    }
}
