using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data.SqlClient;
using System.Net;
using System.IO;
using mshtml;
using NTextCat;
using System.Text.RegularExpressions;

namespace WebInterface
{
    public class SearchEngine
    {
        static Dictionary<string, List<string>> InvertedIndex = new Dictionary<string, List<string>>();
        public List<string> GetURLs()
        {
            SqlConnection cnn;
            cnn = new SqlConnection(@"Data Source=DESKTOP-G7QEFAP\SQLEXPRESS01;Initial Catalog=IR Database;Integrated Security=SSPI");
            cnn.Open();
            List<string> Contents = new List<string>();
            SqlCommand command;

            string sql = @"select URL,content from URLs";
            command = new SqlCommand(sql, cnn);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string content = (string)reader["Content"];
                Contents.Add(content);
            }

            cnn.Close();

            return Contents;
        }
        public void saveCrawlerInfo(string url, string content)
        {
            SqlConnection cnn;
            cnn = new SqlConnection(@"Data Source=DESKTOP-G7QEFAP\SQLEXPRESS01;Initial Catalog=IR Database;Integrated Security=SSPI");
            cnn.Open();
            //do things 
            SqlCommand command;
            string sql = "";

            sql = @"Insert into URLs(URL,Content) values(@url,@content)";
            command = new SqlCommand(sql, cnn);
            SqlParameter parameter1 = new SqlParameter("@URL", url);
            SqlParameter parameter2 = new SqlParameter("@Content", content);
            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);
            command.ExecuteNonQuery();
            //update new URLname and new URLcontent to pass it to database 

            cnn.Close();
        }
        public void saveInvertedIndex(string term, string doc_id, int frequency, string position)
        {
            SqlConnection cnn;
            cnn = new SqlConnection(@"Data Source=DESKTOP-G7QEFAP\SQLEXPRESS01;Initial Catalog=IR Database;Integrated Security=SSPI");
            cnn.Open();
            //do things 
            SqlCommand command;
            string sql = "";

            sql = "Insert into Inverted_Index(term,doc_id,frequency,position) values(@term,@doc_id,@frequency,@position)";
            command = new SqlCommand(sql, cnn);
            SqlParameter parameter1 = new SqlParameter("@term", term);
            SqlParameter parameter2 = new SqlParameter("@doc_id", doc_id);
            SqlParameter parameter3 = new SqlParameter("@frequency", frequency);
            SqlParameter parameter4 = new SqlParameter("@position", position);
            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);
            command.Parameters.Add(parameter3);
            command.Parameters.Add(parameter4);
            command.ExecuteNonQuery();

            cnn.Close();
        }
        public void saveTerms(string termBefore, string doc_id)
        {
            SqlConnection cnn;
            cnn = new SqlConnection(@"Data Source=DESKTOP-G7QEFAP\SQLEXPRESS01;Initial Catalog=IR Database;Integrated Security=SSPI");
            cnn.Open();
            //do things 
            SqlCommand command;
            string sql = "";

            sql = "Insert into Before_Ling(term_before,doc_id) values(@term,@doc_id)";
            command = new SqlCommand(sql, cnn);
            SqlParameter parameter1 = new SqlParameter("@term", termBefore);
            SqlParameter parameter2 = new SqlParameter("@doc_id", doc_id);
            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);
            command.ExecuteNonQuery();

            cnn.Close();
        }
        public void GetTerms(string Term, ref Dictionary<string, List<string>> TermDB)
        {
            List<string> val = new List<string>();
            SqlConnection con;
            con = new SqlConnection(@"Data Source=DESKTOP-G7QEFAP\SQLEXPRESS01;Initial Catalog=IR Database;Integrated Security=SSPI");
            con.Open();
            SqlCommand cmd = new SqlCommand("select term , doc_id, frequency, position from Inverted_Index where term = @t", con);
            SqlParameter parameter = new SqlParameter("@t", Term);
            cmd.Parameters.Add(parameter);
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                val.Add((string)reader["doc_id"]);
                val.Add(reader["frequency"].ToString());
                val.Add((string)reader["position"]);
            }
            TermDB.Add(Term, val);
            con.Close();
        }
        public string GetURL(string doc_id)
        {
            string val = "";
            SqlConnection con;
            con = new SqlConnection(@"Data Source=DESKTOP-G7QEFAP\SQLEXPRESS01;Initial Catalog=IR Database;Integrated Security=SSPI");
            con.Open();
            SqlCommand cmd = new SqlCommand("select URL from URLs where doc_id = @id", con);
            SqlParameter parameter = new SqlParameter("@id", doc_id);
            cmd.Parameters.Add(parameter);
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                val = (string)reader["URL"];
            }
            con.Close();

            return val;
        }
        public string GetText(string rstring)
        {
            string html = rstring;
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            string text = doc.DocumentNode.InnerText;
            text = Regex.Replace(text, @"\s+", " ").Trim();
            return text;
        }

        public List<string> splitDocument(string document)
        {
            List<string> words = new List<string>();
            words = document.Split(' ').ToList();
            return words;
        }
        public void UpdateKey<TKey, TValue>(IDictionary<TKey, TValue> dic, TKey fromKey, TKey toKey)
        {
            TValue value = dic[fromKey];
            dic.Remove(fromKey);
            dic[toKey] = value;
        }
        public void Linguistics()
        {
            PorterStemmer Stemmer = new PorterStemmer();

            List<string> StopWords = new List<string> { "a", "and", "around", "every", "for", "from", "in"
                , "is", "it", "not", "on", "one", "the", "to", "under", "an", "of", "if" ,"are","am","was"
                ,"were","have","had","has","do","did","does","been" ,"be" };
            for (int i = 0; i < InvertedIndex.Keys.Count; i++)
            {
                if (StopWords.Contains(InvertedIndex.Keys.ElementAt(i)))
                {
                    InvertedIndex.Remove(InvertedIndex.Keys.ElementAt(i));
                    continue;
                }
                string UpdatedValue = Stemmer.StemWord(InvertedIndex.Keys.ElementAt(i));
                UpdateKey(InvertedIndex, InvertedIndex.Keys.ElementAt(i), UpdatedValue);
            }
        }
        public List<string> LinguisticsOnSearch(List<string> Words)
        {
            PorterStemmer Stemmer = new PorterStemmer();

            List<string> StopWords = new List<string> { "a", "and", "around", "every", "for", "from", "in"
                , "is", "it", "not", "on", "one", "the", "to", "under", "an", "of", "if" ,"are","am","was"
                ,"were","have","had","has","do","did","does","been" ,"be" };
            for (int i = 0; i < Words.Count; i++)
            {
                if (StopWords.Contains(Words[i]))
                {
                    Words.RemoveAt(i);
                    continue;
                }
                string UpdatedValue = Stemmer.StemWord(Words[i]);
                Words[i] = UpdatedValue;
            }
            return Words;
        }
        public List<string> RemoveSpecialChar(List<string> Words)
        {
            List<string> SpecialChar = new List<string> { "~" , "!" ,"@","$","%","~","^","&","*","#"
                ,"(",")","_","-","+","=","{","}","[","]",";","'",":",".","\\","/","<",">","?","\"" };
            for (int i = 0; i < Words.Count; i++)
            {
                Words[i] = Words[i].ToLower();
                if (SpecialChar.Contains(Words[i]))
                {
                    Words.RemoveAt(i);
                    continue;
                }
                for (int j = 0; j < SpecialChar.Count; j++)
                {
                    if (Words[i].Contains(SpecialChar[j]))
                    {
                        Words[i] = Words[i].Replace(SpecialChar[j], "");
                    }
                }
            }
            return Words;
        }

        public void CreateInvertedIndex(List<string> phrase, int doc_id)
        {
            for (int i = 0; i < phrase.Count; i++)//phrase is the list i am proccessing rn
            {
                if (InvertedIndex.ContainsKey(phrase[i]))
                {
                    InvertedIndex[phrase[i]][0] += ("," + doc_id.ToString()); //count is docID
                    int freq = Int32.Parse(InvertedIndex[phrase[i]][1]);
                    freq++;
                    InvertedIndex[phrase[i]][1] = freq.ToString();
                    InvertedIndex[phrase[i]][2] += ("," + (i + 1).ToString());
                }
                else
                {
                    List<string> currentValue = new List<string>();
                    currentValue.Add(doc_id.ToString());
                    currentValue.Add("1");
                    currentValue.Add((i + 1).ToString());

                    InvertedIndex.Add(phrase[i], currentValue);
                }
            }
        }

        public List<string> GetCommonDoc(string[] List1, string[] List2)
        {
            List<string> CommonDocs = new List<string>();

            int ptr1 = 0, ptr2 = 0;

            while (ptr1 != List1.Length && (ptr2 != List2.Length))
            {
                if (List1[ptr1].Equals(List2[ptr2]))
                {
                    CommonDocs.Add(List1[ptr1]);
                    ptr1++;
                    ptr2++;
                    continue;
                }

                if (Int32.Parse(List1[ptr1]) > Int32.Parse(List2[ptr2]))
                    ptr2++;
                if (Int32.Parse(List1[ptr1]) < Int32.Parse(List2[ptr2]))
                    ptr1++;
            }
            return CommonDocs;
        }
        public List<string> Search(List<string> toSearch)
        {
            List<string> commonDocIDs = new List<string>();
            Dictionary<string, List<string>> Terms = new Dictionary<string, List<string>>();

            for (int i = 0; i < toSearch.Count; i++)
            {
                GetTerms(toSearch[i], ref Terms);
            }
            string[] IDs1 = null;
            List<string> termToCheck1 = null;

            if (toSearch.Count == 1)
            {
                termToCheck1 = Terms[toSearch[0]];
                IDs1 = termToCheck1[0].Split(',');

                string[] DistinctDoc = IDs1.Distinct().ToArray();

                return DistinctDoc.ToList();
            }

            termToCheck1 = Terms[toSearch[0]];
            IDs1 = termToCheck1[0].Split(',');

            List<string> termToCheck2 = Terms[toSearch[1]];
            string[] IDs2 = termToCheck2[0].Split(',');

            commonDocIDs = GetCommonDoc(IDs1, IDs2);

            for (int i = 2; i < toSearch.Count; i++)
            {
                List<string> termToCheck = Terms[toSearch[i]];

                string[] IDs = termToCheck[0].Split(',');

                commonDocIDs = GetCommonDoc(IDs, commonDocIDs.ToArray());
            }
            return commonDocIDs;
        }


        public List<string> ExactSearch(List<string> toSearch)
        {
            Dictionary<string, List<string>> Terms = new Dictionary<string, List<string>>();

            for (int i = 0; i < toSearch.Count; i++)
            {
                GetTerms(toSearch[i], ref Terms);
            }
            List<string> commonDocIDs = new List<string>();
            List<string> CommonDocWithPos = new List<string>();
            List<Dictionary<string, List<string>>> AllDict = new List<Dictionary<string, List<string>>>();
            string[] IDs1 = null;
            List<string> termToCheck1 = null;

            if (toSearch.Count == 1)
            {
                termToCheck1 = Terms[toSearch[0]];
                IDs1 = termToCheck1[0].Split(',');

                string[] DistinctDoc = IDs1.Distinct().ToArray();

                return DistinctDoc.ToList();
            }
            else
            {
                termToCheck1 = Terms[toSearch[0]];
                IDs1 = termToCheck1[0].Split(',');

                List<string> termToCheck2 = Terms[toSearch[1]];
                string[] IDs2 = termToCheck2[0].Split(',');

                commonDocIDs = GetCommonDoc(IDs1, IDs2);

                for (int i = 2; i < toSearch.Count; i++)
                {
                    List<string> termToCheck = Terms[toSearch[i]];

                    string[] IDs = termToCheck[0].Split(',');

                    commonDocIDs = GetCommonDoc(IDs, commonDocIDs.ToArray());
                }

                for (int k = 0; k < Terms.Count; k++)
                {
                    Dictionary<string, List<string>> DocChecked = new Dictionary<string, List<string>>();
                    List<string> termToCheck = Terms[toSearch[k]];
                    string[] IDs = termToCheck[0].Split(',');
                    string[] pos = termToCheck[2].Split(',');
                    for (int i = 0; i < commonDocIDs.Count; i++)
                    {
                        for (int j = 0; j < IDs.Length; j++)
                        {
                            if (IDs[j] == commonDocIDs[i])
                            {
                                if (DocChecked.Keys.Contains(commonDocIDs[i]))
                                {
                                    DocChecked[commonDocIDs[i]].Add(pos[j]);
                                }
                                else
                                {
                                    List<string> Positions = new List<string>();
                                    Positions.Add(pos[j]);
                                    DocChecked.Add(commonDocIDs[i], Positions);
                                }
                            }
                        }
                    }
                    AllDict.Add(DocChecked);
                }

                for (int l = 0; l < AllDict[0].Keys.Count; l++)
                {
                    List<string> Doc = AllDict[0][AllDict[0].Keys.ElementAt(l)];
                    bool seq = true;
                    for (int j = 0; j < Doc.Count; j++)
                    {
                        int Pos = Int32.Parse(Doc[j]);
                        for (int k = 1; k < AllDict.Count; k++)
                        {
                            if (AllDict[k][AllDict[k].Keys.ElementAt(l)].Contains((Pos + 1).ToString()))
                            {
                                Pos++;
                            }
                            else
                            {
                                seq = false;
                                break;
                            }
                        }
                        if (seq == true)
                        {
                            CommonDocWithPos.Add(AllDict[0].Keys.ElementAt(l));
                        }
                    }
                }
            }
            return CommonDocWithPos;
        }
        public List<string> CheckPositions(List<string> List1, List<string> List2)
        {
            List<string> Positions = new List<string>();

            int ptr1 = 0, ptr2 = 0;

            while (ptr1 != List1.Count && (ptr2 != List2.Count))
            {
                if (Int32.Parse(List1[ptr1]) == Int32.Parse(List2[ptr2]) + 1)
                {
                    Positions.Add(List1[ptr1]);
                    ptr1++;
                    ptr2++;
                    continue;
                }

                if (Int32.Parse(List1[ptr1]) > Int32.Parse(List2[ptr2]))
                    ptr2++;
                if (Int32.Parse(List1[ptr1]) < Int32.Parse(List2[ptr2]))
                    ptr1++;
            }
            return Positions;
        }
        public void Crawler()
        {
            Queue<string> URL_links = new Queue<string>();

            String URL = "https://www.bbc.com";

            URL_links.Enqueue(URL);
            int counter = 0;
            while (URL_links.Count != 0)
            {
                // Create a new 'WebRequest' object to the mentioned URL.
                string currentURL = URL_links.Dequeue();
                Console.WriteLine("Current" + currentURL);

                WebRequest myWebRequest;
                WebResponse myWebResponse;
                try
                {
                    myWebRequest = WebRequest.Create(currentURL);
                    myWebResponse = myWebRequest.GetResponse();
                }
                catch (Exception e)
                {
                    continue;
                }

                Stream streamResponse = myWebResponse.GetResponseStream();
                StreamReader sReader = new StreamReader(streamResponse);
                string rString = sReader.ReadToEnd();

                sReader.Close();
                myWebResponse.Close();
                counter++;
                if (counter == 3000)
                    break;

                Console.WriteLine("DB: " + currentURL);

                IHTMLDocument2 myDoc = new HTMLDocumentClass();
                myDoc.write(rString);
                IHTMLElementCollection elements = myDoc.links;
                foreach (IHTMLElement el in elements)
                {
                    string link = (string)el.getAttribute("href", 0);
                    if (link.Contains("about:"))
                    {
                        link = link.Replace("about:", URL);
                    }
                    try
                    {
                        WebRequest request = WebRequest.Create(link);
                        WebResponse response = request.GetResponse();
                        string type = response.ContentType;

                        if (!(type.Contains("UTF-8") || type.Contains("utf-8")))
                            continue;

                        if (URL_links.Contains(link))
                            continue;

                        URL_links.Enqueue(link);

                        Console.WriteLine(link);
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }
            }
        }
        public void Indexer()
        {
            List<string> Contents = GetURLs();
            List<string> Docs = new List<string>();
            List<List<string>> Words = new List<List<string>>();
            int count = 0;

            for (int i = 0; i < Contents.Count; i++)
            {
                Docs.Add(GetText(Contents[i]));
            }
            for (int i = 0; i < Docs.Count; i++)
            {
                List<string> temp = splitDocument(Docs[i]);
                Words.Add(temp);
                count += temp.Count;
            }
            for (int i = 0; i < Words.Count; i++)
            {
                Words[i] = RemoveSpecialChar(Words[i]);
                CreateInvertedIndex(Words[i], i + 1);
            }
            for (int i = 0; i < InvertedIndex.Keys.Count; i++)
            {
                saveTerms(InvertedIndex.Keys.ElementAt(i)
                , InvertedIndex[InvertedIndex.Keys.ElementAt(i)][0]);
            }
            Linguistics();

            for (int i = 0; i < InvertedIndex.Keys.Count; i++)
            {
                saveInvertedIndex(InvertedIndex.Keys.ElementAt(i)
                , InvertedIndex[InvertedIndex.Keys.ElementAt(i)][0]
                , Int32.Parse(InvertedIndex[InvertedIndex.Keys.ElementAt(i)][1])
                , InvertedIndex[InvertedIndex.Keys.ElementAt(i)][2]);
            }
        }
    }
}