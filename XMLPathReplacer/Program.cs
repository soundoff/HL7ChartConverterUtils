using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Globalization;

namespace XMLPathReplacer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Please pick a task:\r\n\t1) Change file name in PATH element" + 
                    "\r\n\t2) Set element value to portion of filename" +
                    "\r\n\t3) Convert date value to MM/DD/YYYY format");
                int task = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("\r\n");
                Console.WriteLine("Enter the full path of the xml you want to change the Path element value for:");
                string xmlpath = Console.ReadLine();
                Console.WriteLine("\r\n");
                XDocument config = XDocument.Load(xmlpath);
                switch (task)
                {
                    case 1:
                        Console.WriteLine("This utility will fix a long filename in the path\r\n" +
                            "\telement of an HL7 import xml file.");
                        Console.WriteLine("The full path will be kept intact, while the \r\n\t filename is shortened by the amount you input.");
                        Console.WriteLine("\r\n");
                        Console.WriteLine("Enter the number of characters you wish to\r\n" +
                            "\tshorten the current file name to:");
                        int newLenth = Convert.ToInt32(Console.ReadLine());
                        var moose = from path in config.Root.Descendants("Path")
                                    let length = path.Value.GetNameNoExtension().Length
                                    where length > newLenth
                                    select path;

                        foreach (var path in moose)
                        {
                            string newValue = FixPath(path.Value, newLenth);
                            Console.WriteLine("Changing\r\n{0}\r\nto\r\n{1}", path.Value, newValue);
                            path.SetValue(newValue);
                        }

                        break;
                    case 2:
                        Console.WriteLine("This utility will set a specified element value\r\n" +
                            "to a specified portion of the filename starting at character zero");
                        Console.WriteLine("\r\n");
                        Console.WriteLine("Enter the element name you wish to change:");
                        string ElementName = Console.ReadLine();
                        Console.WriteLine("\r\n");
                        Console.WriteLine("Enter the number of characters to extract for element value:");
                        int ExtractText = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("\r\n");
                        var elDate = from path in config.Root.Descendants("DateTime")
                                    let length = path.Value.GetNameNoExtension().Length
                                    where length > ExtractText
                                    select path;

                        var elPath = from path in config.Root.Descendants("Notes")
                                    let length = path.Value.GetNameNoExtension().Length
                                    where length > ExtractText
                                    select path;

                        XElement[] Dates = elDate.ToArray();
                        XElement[] Paths = elPath.ToArray();

                        for (int i = 0; i <= Dates.GetUpperBound(0); i++ )
                        {
                            string newValue = SubDateToElementValue(Paths[i].Value, ExtractText);
                            Console.WriteLine("Extracting\r\n{0}\r\nfrom\r\n{1}", newValue, Dates[i].Value);
                            Dates[i].SetValue(newValue);
                        }
                        break;
                    case 3:
                        Console.WriteLine("This utility will convert a date value to the format\r\n" +
                            "MM/DD/YYYY.  If the conversion fails, it will leave it the way it was.");
                        Console.WriteLine("\r\n");
                        var elDates = from dateNode in config.Root.Descendants("DateTime")
                                      select dateNode;

                        XElement[] DateElement = elDates.ToArray();

                        for (int i = 0; i <= DateElement.GetUpperBound(0); i++)
                        {
                            string newValue = FixDateFormat(DateElement[i].Value);
                            Console.WriteLine("Converting\r\n{0}\r\nto\r\n{1}", DateElement[i].Value, newValue);
                            DateElement[i].SetValue(newValue);
                        }
                        break;

                    default:
                        break;
                }
                config.Save(xmlpath);
                Console.WriteLine("Done");
                Console.ReadLine();
            }
            catch (FileNotFoundException FileNotFoundEx)
            {
                Console.WriteLine("\r\n");
                Console.WriteLine("The path you entered did not exist.\r\nPlease exit and try again.");
                Console.WriteLine("\r\n");
                Console.WriteLine(FileNotFoundEx.ToString());
                Console.WriteLine("Press ENTER to end the program...");
                Console.ReadLine();
            }
            catch (XmlException xmlEX)
            {
                Console.WriteLine("\r\n");
                Console.WriteLine("The following XML error happened while working with the document:");
                Console.WriteLine("\r\n");
                Console.WriteLine(xmlEX.ToString());
                Console.WriteLine("\r\n");
                Console.WriteLine("Press ENTER to end the program...");
                Console.ReadLine();
            }
            catch (Exception GenericEx)
            {
                Console.WriteLine("\r\n");
                Console.WriteLine("The following general error happened while working with the XML document:");
                Console.WriteLine("\r\n");
                Console.WriteLine(GenericEx.ToString());
                Console.WriteLine("\r\n");
                Console.WriteLine("Press ENTER to end the program...");
                Console.ReadLine();
            }
        }

        static string FixPath(string old, int length)
        {
            StringBuilder shiny = new StringBuilder();
            shiny.Append(old.GetPath());
            shiny.Append(@"\");
            shiny.Append(old.GetNameNoExtension().Substring(0, length));
            shiny.Append(".tif");
            return shiny.ToString();
        }

        static string SubDateToElementValue(string FileName, int Length)
        {
            string piece = FileName.GetNameNoExtension().Substring(0, Length);
            piece = piece.Replace("_", "/");
            try
            {
                DateTime dtPiece = Convert.ToDateTime(piece);
                return dtPiece.ToString();
            }
            catch
            {
                DateTime dtPiece = Convert.ToDateTime(null);
                return dtPiece.ToString();
            }
        }

        static string FixDateFormat(string OldDateFormat)
        {
            try
            {
                DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();
                dtFormat.ShortDatePattern = "MM/dd/yyyy";
                DateTime dtPiece = Convert.ToDateTime(OldDateFormat,dtFormat);
                return dtPiece.ToString("MM/dd/yyyy");
            }
            catch
            {
                return OldDateFormat;
            }
        }
    }

    public static class Helper
    {
        public static string GetPath(this string FilePath)
        {
            return Path.GetDirectoryName(FilePath);
        }

        public static string GetNameNoExtension(this string FileName)
        {
            return Path.GetFileNameWithoutExtension(FileName);
        }

    }
}
