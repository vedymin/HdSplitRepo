using IBM.Data.DB2.iSeries;
using System;
using System.Threading.Tasks;
using HdSplit.ViewModels;

namespace HdSplit.Models
{
    public class ReflexConnectionModel
    {
        private string _queryString;
        iDB2Connection conn = new iDB2Connection ("DataSource=10.52.1.100; UserID=VASPROD; Password=VASPROD;");

        public HdModel OriginalHdModel { get; set; } = new HdModel(false);

        public void DownloadHdFromReflex(string _hd) {
            
            Console.WriteLine ("Trying to connect to AS400 DB2 using Client Access .dll");
            try {
                conn.Open ();
                if (conn != null) {
                    Console.WriteLine ("Successfully connected...");
                    _queryString = "SELECT GENSUP, GECART, GECQAL, A2CFAR, GEQGEI FROM GUEPRDDB.HLGEINP " + 
                                   "inner join GUEPRDDB.HLCDFAP on GECART = A2CART " + 
                                   $"WHERE GENSUP = '{_hd}' and A2CFAN = 'LINE'";
                    iDB2Command comm = conn.CreateCommand ();
                    comm.CommandText = _queryString;
                    iDB2DataReader reader = comm.ExecuteReader ();
                    
                    while (reader.Read ()) {
                        OriginalHdModel.ListOfIpgs.Add(new IpgModel() 
                            {
                                Item = reader.GetString(1),
                                Grade = reader.GetString (2),
                                Line = (Lines)Enum.Parse(typeof(Lines), reader.GetString(3)),
                                Quantity = reader.GetInt32(4),
                                //UpcCode = reader.GetString (5),
                        });
                    }
                    reader.Close ();
                    comm.Dispose ();
                    
                }

            } catch (Exception ex) {
                Console.WriteLine ("Error : " + ex);
                Console.WriteLine (ex.StackTrace);
            } finally {
                conn.Close ();
            }
            
        }

        public void OpenConnection()
        {
            conn.Open ();
        }

        public void CloseConnection()
        {
            conn.Close ();
        }

        public string DownloadUpcForItemsAsync(string _item)
        {
            string result = "error";
            Console.WriteLine ("Trying to connect to AS400 DB2 using Client Access .dll");
            try
            {
                
                if (conn != null)
                {
                    Console.WriteLine ("Successfully connected...");
                    _queryString = $"SELECT VICIVL FROM GUEPRDDB.HLVLIDP WHERE VICART = '{_item}'";
                    iDB2Command comm = conn.CreateCommand ();
                    comm.CommandText = _queryString;
                    iDB2DataReader reader = comm.ExecuteReader ();

                    while (reader.Read())
                    {
                        result = reader.GetString(0).ToString().Trim();
                    }

                    Console.WriteLine ("Upc added");


                    reader.Close ();
                    comm.Dispose ();


                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
            return result;


        }
    }
}