using IBM.Data.DB2.iSeries;
using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using HdSplit.ViewModels;

namespace HdSplit.Models
{
    public class ReflexConnectionModel
    {
        // Consider throwing this into configuration file.
        iDB2Connection conn = new iDB2Connection ("DataSource=10.52.1.100; UserID=VASPROD; Password=VASPROD;");


        public HdModel OriginalHdModel { get; set; } = new HdModel(false);

        /// <summary>
        /// Returns true if HD exist, and saves this hd in OriginalHdModel instance created in this class.
        /// If hd is Unknown returns false.
        /// </summary>
        public bool DownloadHdFromReflex(string _hd) 
        {
            Console.WriteLine ("Trying to connect to Reflex for downloading HD informations.");

            try 
            {
                conn.Open ();
                if (conn != null) 
                {
                    Console.WriteLine ("Successfully connected to Reflex for downloading HD informations.");

                    // Below are DB2 functions needed for executing query
                    // Query join table where we can see lines by items. A2CFAN is telling to DB2 to show only lines value.
                    // You can change it to show CAPO or something else. Result will be in field A2CFAR.
                    // This also needs to be ordered by Item.
                    string _queryString = "SELECT GECART, GECQAL, A2CFAR, GEQGEI FROM GUEPRDDB.HLGEINP " + 
                                   "inner join GUEPRDDB.HLCDFAP on GECART = A2CART " + 
                                   $"WHERE GENSUP = '{_hd}' and A2CFAN = 'LINE' " +
                                   "Order by GECART";
                    iDB2Command comm = conn.CreateCommand ();
                    comm.CommandText = _queryString;
                    iDB2DataReader reader = comm.ExecuteReader ();
                    
                    // Below if checks if there is some data in result. If no then it return false.
                    // That means that HD is Unknown.
                    if (reader.HasRows)
                    {
                        // Reader in while goes through all rows of results from Reflex.
                        while (reader.Read ()) 
                        {
                            // Here we are adding new IPG to HD object.
                            OriginalHdModel.ListOfIpgs.Add (new IpgModel () 
                            {
                                Item = reader.GetString (0),
                                Grade = reader.GetString (1),
                                // Lines is an enum so we need parse string to enum here.
                                Line = (Lines)Enum.Parse (typeof (Lines), reader.GetString (2)),
                                Quantity = reader.GetInt32 (3),
                            });
                        }

                        // some cleaning.
                        reader.Close ();
                        comm.Dispose ();

                        // Returns true so we have our data in "OriginalHdModel" instance.
                        return true;
                    }
                    else
                    {
                        // When there is no data from Reflex
                        return false;
                    }
                }

            } 
            catch (Exception ex) 
            {
                Console.WriteLine ("Error : " + ex);
                Console.WriteLine (ex.StackTrace);
                return false;
            } 
            finally 
            {
                conn.Close ();
            }

            // This will never reach by needs to be here because of error "Not all is returning value".
            return false;
        }

        /// <summary>
        /// Returns downloaded bindableCollection of Ipg's with UPC codes matching items.
        /// </summary>
        public BindableCollection<IpgModel> DownloadUpcForItemsAsync(BindableCollection<IpgModel> _ipgsCollection)
        {
            Console.WriteLine ("Trying to connect to Reflex for downloading UPC codes...");

            // Preparing a formatted list of items.
            string _items = ConcatenateItemsIntoList (_ipgsCollection);

            try
            {
                conn.Open();
                if (conn != null)
                {
                    Console.WriteLine("Successfully connected to Reflex for downloading UPC codes");

                    // Below are DB2 functions needed for executing query
                    string _queryString = $"SELECT VICIVL FROM GUEPRDDB.HLVLIDP WHERE VICART IN {_items} and VICTYI = 'EAN_1' Order by VICIVL ";
                    iDB2Command comm = conn.CreateCommand();
                    comm.CommandText = _queryString;
                    iDB2DataReader reader = comm.ExecuteReader();

                    // countOfIpg is index needed for going through _ipgsCollection.
                    int countOfIpg = 0;

                    // Reader in while goes through all rows of results from Reflex.
                    while (reader.Read())
                    {
                        // Here we use index to assign row by row result from Reflex to UpcCode of IPG in _ipgsCollection
                        // It is important tha items needs to be ordered both in object of hd and in the result list.
                        // For that we are using "order by" in our sql.
                        _ipgsCollection[countOfIpg].UpcCode = reader.GetString(0).ToString().Trim();
                        countOfIpg++;
                    }

                    Console.WriteLine("Upc downloaded");
                    
                    // Some cleaning needed.
                    reader.Close();
                    comm.Dispose();

                    // We are returning whole collection straight to foreach loop so we can assign again to
                    // our ViewModel object of HD. For that check ShellViewModel class.
                    return _ipgsCollection;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
            finally
            {
                conn.Close ();
            }

            // This will never reach by needs to be here because of error "Not all is returning value".
            return _ipgsCollection;
        }

        /// <summary>
        /// This function takes collection of IPG's and concatenate it's items into list sourrunded
        /// by parenthesis: ('761...','761...,'761...')  Returns concatened list of items in string format.
        private string ConcatenateItemsIntoList(BindableCollection<IpgModel> _ipgsCollection)
        {
            string items = "(";

            foreach (var index in _ipgsCollection) {
                items += "'" + index.Item.Trim () + "',";
            }

            // Removes last character which is now ","
            items = items.Remove (items.Length - 1);
            items += ")";

            return items;
        }
    }
}