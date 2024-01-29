// to put into message.cs


using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
//Created because the Comment segment was missing // GD 21 january 2024
// I copy the same as Result.cs
// I saw you created the public class CommentRecordSet in YamlSetting.cs but It seems that you don't use this class
//using System.Linq; // added by default by MVStudio
//using System.Text; // added by default by MVStudio
//using System.Threading.Tasks; // added by default by MVStudio

namespace UniversaLIS
{
    public class Comment
    {
        public Dictionary<string, string> Elements = new Dictionary<string, string>();

        public string CommentMessage
        {
            get => GetResultString();
            set => SetResultString(value);
        }
        private string GetResultString()
        {
            string[] elementArray = { "FrameNumber", "Sequence #", "Comment Source", "Comment Text", "Comment Type"};
            foreach (var item in elementArray)
            {
                if (!Elements.ContainsKey(item))
                {
                    Elements.Add(item, "");
                }
            }
            string output = Constants.STX + Elements["FrameNumber"].Trim('C') + "C|";
            // Concatenate the Dictionary values and return the string.
            output += Elements["Sequence #"] + "|";
            output += Elements["Comment Source"] + "|";
            output += Elements["Comment Text"] + "|";
            output += Elements["Comment Type"]+ Constants.CR + Constants.ETX;
            return output;

        }

        private void SetResultString(string input)
        {
            string[] inArray = input.Split('|');
            if (inArray.Length < 5)      //GD 27 01 2023 sometime the LIS or intrument doesn't send all the frame because there all data are not requiered. 
                                         // example sent <STX>3O|1|25028||^^^DIF|||||||||||||||||||||F<CR><ETX>13<CR><LF>
                                         // instead of   <STX>3O|1|25028||^^^DIF|||||||||||||||||||||F|||||<CR><ETX>13<CR><LF>
                                         // or           <STX>3C|1|I|Patient Comment<CR><ETX>3C<CR><LF>
                                         // instead of   <STX>3C|1|I|Patient Comment|<CR><ETX>3C<CR><LF>
                                         // the exception is not necessary. It can normal to receive shorter frame.
            {
                // Invalid number of elements.
                throw new Exception($"Invalid number of elements in result record string. Expected: 5 \tFound: {inArray.Length} \tString: \n{input}");
            }
            Elements["FrameNumber"] = inArray[0];
            Elements["Sequence #"] = inArray[1];
            Elements["Comment Source"] = inArray[2];
            Elements["Comment Text"] = inArray[3];
            Elements["Comment Type"] = inArray[4].Substring(0, inArray[4].IndexOf(Constants.CR));
        }
        public Comment(string CommentMessage)
        {
            SetResultString(CommentMessage);
        }
        
        /*
    1 Record Type C or FrameNumber
    2 Sequence or  Nb 1, 2, ...
    3 Comment Source I clinical instrument system
    4 Comment Text
    5 Comment Type G:Free text ,I: Instrument flag comment ,L: Comment from host (Patient order)


    */

    }

}
