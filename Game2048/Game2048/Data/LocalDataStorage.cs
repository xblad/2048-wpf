using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Diagnostics;

using Game2048.Objects;
using Game2048.Other;

namespace Game2048.Data
{
    public class LocalDataStorage
    {

        private static string GetPathToBackupXML() // save file full path
        {
            //return System.IO.Path.GetFullPath(@"..\..\xml\scores.xml");
            return Path.Combine(GetXMLDirectory(), @"save_file.xml");
        }

        private static string GetXMLDirectory() // save file full directory
        {
            return Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData), @"2048");
        }

        public static GameBoard.GameState GetGameState()
        {
            GameBoard.GameState gameState = GameBoard.GameState.NewGame; // default game state
            if (File.Exists(GetPathToBackupXML()))
            {
                XDocument doc = XDocument.Load(GetPathToBackupXML());
                Enum.TryParse(doc.Element("game").Attribute("currentGameState").Value, out gameState); // success try return game state from local storage
            }
            else { CreateSaveFileXML(); }

            return gameState;
        }

        public static int[,] GetTileMatrix() // returns multidimensional array of tile matrix from local storage
        {
            int size = GamePage.GAME_BOARD_SIZE;
            int[,] tileMatrix = new int[size, size];
            if (File.Exists(GetPathToBackupXML()))
            {
                XDocument doc = XDocument.Load(GetPathToBackupXML());
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        tileMatrix[i, j] = int.Parse(doc.Element("game")
                            .Element("tileMatrix")
                            .Elements("cell")
                            .Where(t => t.Attribute("row").Value == i.ToString() && t.Attribute("column").Value == j.ToString())
                            .SingleOrDefault().Value); // returns zero or value in cell
                    }
                }
            }
            else { CreateSaveFileXML(); }

            return tileMatrix;
        }
        
        public static long GetCurrentScore() // returns current score from local storage
        {
            long currentScore = 0;
            if (File.Exists(GetPathToBackupXML()))
            {
                XDocument doc = XDocument.Load(GetPathToBackupXML());
                long.TryParse(doc.Element("game").Attribute("currentScore").Value, out currentScore);
            }
            else { CreateSaveFileXML(); }

            return currentScore;
        }

        public static long GetLastCurrentScore() // returns last known current score from local storage
        {
            long lastCurrentScore = 0;
            if (File.Exists(GetPathToBackupXML()))
            {
                XDocument doc = XDocument.Load(GetPathToBackupXML());
                long.TryParse(doc.Element("game").Attribute("lastCurrentScore").Value, out lastCurrentScore);
            }
            else { CreateSaveFileXML(); }

            return lastCurrentScore;
        }
        
        public static long GetBestScore() // returns best score from local storage
        {
            long bestScore = 0;
            if (File.Exists(GetPathToBackupXML()))
            {
                XDocument doc = XDocument.Load(GetPathToBackupXML());
                long.TryParse(doc.Element("game").Attribute("bestScore").Value, out bestScore);
            }
            else { CreateSaveFileXML(); }

            return bestScore;
        }

        public static void SetBestScore(long bestScore) // save best score to local storage
        {
            if (File.Exists(GetPathToBackupXML()))
            {
                XDocument doc = XDocument.Load(GetPathToBackupXML());
                doc.Element("game").Attribute("bestScore").Value = bestScore.ToString();
                doc.Save(GetPathToBackupXML());
            }
            else { CreateSaveFileXML(); }
        }

        public static Guid GetGamerGuid() // returns gamer guid
        {
            Guid gamerGuid = Guid.Empty; // default guid in case local storage doesn't contains any
            if (File.Exists(GetPathToBackupXML()))
            {
                XDocument doc = XDocument.Load(GetPathToBackupXML());
                Guid.TryParse(doc.Element("game").Attribute("gamerGuid").Value, out gamerGuid); // success try returns gamer Guid from local storage
            }
            else { CreateSaveFileXML(); }

            return gamerGuid;
        }

        public static void SetGamerGuid(Guid gamerGuid) // save gamer Guid to local storage
        {
            if (File.Exists(GetPathToBackupXML()))
            {
                XDocument doc = XDocument.Load(GetPathToBackupXML());
                doc.Element("game").Attribute("gamerGuid").Value = gamerGuid.ToString();
                doc.Save(GetPathToBackupXML());
            }
            else { CreateSaveFileXML(); }
        }

        public static void CreateSaveFileXML() // new empty save file
        {
            System.IO.Directory.CreateDirectory(GetXMLDirectory());
            
            XDocument doc = new XDocument(
                new XDeclaration("1.0", Encoding.UTF8.HeaderName, String.Empty),
                new XComment("Game backup file"),
                new XElement("game",
                    new XAttribute("bestScore", 0),
                    new XAttribute("currentScore", 0),
                    new XAttribute("lastCurrentScore",0),
                    new XAttribute("gamerGuid",0),
                    new XElement("tileMatrix"),
                    new XAttribute("currentGameState", GameBoard.GameState.NewGame)
                )
            );
            doc.Save(GetPathToBackupXML());
        }

        public static void RefreshSaveFileXML() // store all actual data to local storage
        {
            Directory.CreateDirectory(GetXMLDirectory());
            XDocument doc = XDocument.Load(GetPathToBackupXML());
            if (GameBoard.CurrentGameState != GameBoard.GameState.GameOver)
            {
                doc.Element("game").Attribute("bestScore").Value = GameBoard.GetBestScore().ToString();
                doc.Element("game").Attribute("currentScore").Value = GameBoard.GetCurrentScore().ToString();
                doc.Element("game").Attribute("lastCurrentScore").Value = GameBoard.GetCurrentScore().ToString();
                doc.Element("game").Element("tileMatrix").RemoveAll();
                doc.Element("game").Element("tileMatrix").Add(MatrixOperations.ConvertToXElementsArray(GameBoard.GetGridTileMatrix()));
                doc.Element("game").Attribute("currentGameState").Value = GameBoard.CurrentGameState.ToString();
            }
            else
            {
                doc.Element("game").Attribute("bestScore").Value = GameBoard.GetBestScore().ToString();
                doc.Element("game").Attribute("lastCurrentScore").Value = GameBoard.GetCurrentScore().ToString();
            }
            doc.Save(GetPathToBackupXML());
        }

        public static void ClearSaveFileBeforeNewGame() // clear save file (except best score and gamer Guid)
        {
            System.IO.Directory.CreateDirectory(GetXMLDirectory());
            if (File.Exists(GetPathToBackupXML()))
            {
                XDocument doc = XDocument.Load(GetPathToBackupXML());
                doc.Element("game").Attribute("currentScore").Value = "0";
                doc.Element("game").Attribute("lastCurrentScore").Value = GameBoard.GetCurrentScore().ToString();
                doc.Element("game").Element("tileMatrix").RemoveAll();
                doc.Element("game").Attribute("currentGameState").Value = GameBoard.GameState.GameOver.ToString();
                doc.Save(GetPathToBackupXML());
            }
        }
    }
}
