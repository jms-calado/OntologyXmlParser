using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Xml;
using static OntologyXmlParser.GlobalVars;

namespace OntologyXmlParser
{
    class FileAnalyser
    {
        XmlReader xmlfilereader;

        private void Analyser(string path, string filename)
        {
            string student = string.Empty;
            string session = string.Empty;
            string sensor = string.Empty;
            double Attention = 0, Blink = 0, Lpupil = 0, Rpupil = 0, FixDur = 0;
            double Attention_Result = 0, starting_time = 0, Duration = 0;
            double Anger = 0, Contempt = 0, Disgust = 0, Engagement = 0, Fear = 0, Joy = 0, Sadness = 0, Surprise = 0;
            int Sample_count = 0;
            bool run_1 = true;//first run reading xml first REC
            xmlfilereader = XmlReader.Create(path);
            string data = string.Empty;
            string[] strsplit = filename.Split(' ');
            //foreach(string str in strsplit)Console.WriteLine(str);
            DateTime dateVal = new DateTime();
            try{
                dateVal = DateTime.ParseExact(strsplit[1], "yyyy-MM-dd--HH_mm_ss", CultureInfo.InvariantCulture);
            }
            catch(FormatException er)
            {
                Console.WriteLine("Error: {0}", er);
                return;                
            }
            double Start_Time = ConvertToUnixTimestamp(dateVal);
            if (strsplit[0] == "GP3")
            {
                #region xml
                xmlfilereader = XmlReader.Create(path);
                do
                {
                    try
                    {
                        xmlfilereader.Read();
                    }
                    catch (Exception er)
                    {
                        Console.WriteLine("Error: {0}", er);
                        break;
                    }
                    if ((xmlfilereader.NodeType == XmlNodeType.Element) && (xmlfilereader.Name == "REC"))
                    {
                        double rec_time = 0;
                        try
                        {
                            rec_time = double.Parse(xmlfilereader.GetAttribute("TIME"), CultureInfo.InvariantCulture);
                        }
                        catch (ArgumentNullException e)
                        {
                            Console.WriteLine("Attribute name is null: {0}", e);
                        }
                        if (run_1)
                        {
                            starting_time = rec_time;
                            run_1 = false;
                        }
                        //Duration = Math.Round(rec_time - starting_time, 1);
                        Duration = rec_time - starting_time;
                        if (!run_1)
                        {
                            float fpogx = 0;
                            float fpogy = 0;
                            try
                            {
                                fpogx = float.Parse(xmlfilereader.GetAttribute("FPOGX"), CultureInfo.InvariantCulture);
                            }
                            catch (ArgumentNullException e)
                            {
                                Console.WriteLine("Attribute name is null: {0}", e);
                            }
                            try
                            {
                                fpogy = float.Parse(xmlfilereader.GetAttribute("FPOGY"), CultureInfo.InvariantCulture);
                            }
                            catch (ArgumentNullException e)
                            {
                                Console.WriteLine("Attribute name is null: {0}", e);
                            }
                            double temp = Math.Sqrt((fpogx - 0.5) * (fpogx - 0.5) + (fpogy - 0.5) * (fpogy - 0.5));
                            if (temp > 1) temp = 1;//2.13
                                                   //Attention = 100 - Math.Round(100 * temp);
                            Attention = 1 - temp;
                            try
                            {
                                Blink = float.Parse(xmlfilereader.GetAttribute("BKPMIN"), CultureInfo.InvariantCulture);
                            }
                            catch (ArgumentNullException e)
                            {
                                Console.WriteLine("Attribute name is null: {0}", e);
                            }
                            try
                            {
                                Lpupil = 1000 * float.Parse(xmlfilereader.GetAttribute("LPUPILD"), CultureInfo.InvariantCulture);
                            }
                            catch (ArgumentNullException e)
                            {
                                Console.WriteLine("Attribute name is null: {0}", e);
                            }
                            try
                            {
                                Rpupil = 1000 * float.Parse(xmlfilereader.GetAttribute("RPUPILD"), CultureInfo.InvariantCulture);
                            }
                            catch (ArgumentNullException e)
                            {
                                Console.WriteLine("Attribute name is null: {0}", e);
                            }
                            try
                            {
                                FixDur = 1000 * float.Parse(xmlfilereader.GetAttribute("FPOGD"), CultureInfo.InvariantCulture);
                            }
                            catch (ArgumentNullException e)
                            {
                                Console.WriteLine("Attribute name is null: {0}", e);
                            }
                            Attention_Result += Attention;
                            Sample_count++;
                        }
                    }
                }
                while (!xmlfilereader.EOF);
                #endregion xml
                Attention_Result = Attention_Result / Sample_count;
                //Duration = Math.Round(Duration * 1000, 0);
                Duration = Math.Round(Duration, 0);
                
                //convert absolute time to current time:
                var Digi_Date_Time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Start_Time);
                //convert duration from miliseconds to HH:mm:ss
                TimeSpan Digi_Duration = TimeSpan.FromMilliseconds(Duration);

                ObservationObject observation = new ObservationObject();
                observation.Duration = Digi_Duration.ToString(@"hh\:mm\:ss");
                observation.Date_Time = Digi_Date_Time.ToString("yyyy-MM-ddTHH:mm:ss");
                observation.Sensory_Component = "GP3";
                BehaviourObject behaviour = new BehaviourObject();
                behaviour.On_Task = Attention_Result.ToString(CultureInfo.InvariantCulture);
                behaviour.Off_Task = (1 - Attention_Result).ToString(CultureInfo.InvariantCulture);

                RestAPI.ProcessPostingObjects(observation, null, behaviour, null);
            }
            else if (strsplit[0] == "Affectiva")
            {
                #region xml2
                xmlfilereader = XmlReader.Create(path);
                do
                {
                    try
                    {
                        xmlfilereader.Read();
                    }
                    catch (Exception er)
                    {
                        Console.WriteLine("Error: {0}", er);
                        break;
                    }
                    if ((xmlfilereader.NodeType == XmlNodeType.Element) && (xmlfilereader.Name == "user_id"))
                    {
                        try
                        {
                            string value = xmlfilereader.GetAttribute("value");
                            if (value != string.Empty) student = value;
                            else return;
                        }
                        catch (ArgumentNullException e)
                        {
                            Console.WriteLine("Attribute name is null: {0}", e);
                        }
                    }
                    if ((xmlfilereader.NodeType == XmlNodeType.Element) && (xmlfilereader.Name == "dateTime"))
                    {
                        try
                        {
                            string value = xmlfilereader.GetAttribute("value");
                            if (value != string.Empty) 
                            {
                                DateTime dateVal2 = new DateTime();
                                try{
                                    dateVal2 = DateTime.ParseExact(value, "yyyy-MM-dd--HH:mm:ss", CultureInfo.InvariantCulture);
                                }
                                catch(FormatException er)
                                {
                                    Console.WriteLine("Error: {0}", er);
                                    return;                
                                }
                                if(dateVal.CompareTo(dateVal2) != 0)
                                {
                                     Console.WriteLine("dates don't match");
                                     return;
                                }
                            }
                        }
                        catch (ArgumentNullException e)
                        {
                            Console.WriteLine("Attribute name is null: {0}", e);
                        }
                    }
                    if ((xmlfilereader.NodeType == XmlNodeType.Element) && (xmlfilereader.Name == "session"))
                    {
                        try
                        {
                            string value = xmlfilereader.GetAttribute("value");
                            if (value != string.Empty) session = value;
                            else return;
                        }
                        catch (ArgumentNullException e)
                        {
                            Console.WriteLine("Attribute name is null: {0}", e);
                        }
                    }
                    if ((xmlfilereader.NodeType == XmlNodeType.Element) && (xmlfilereader.Name == "sensor"))
                    {
                        try
                        {
                            string value = xmlfilereader.GetAttribute("value");
                            if (value != string.Empty) sensor = value;
                            else return;
                        }
                        catch (ArgumentNullException e)
                        {
                            Console.WriteLine("Attribute name is null: {0}", e);
                        }
                    }
                    double rec_time = 0;
                    if ((xmlfilereader.NodeType == XmlNodeType.Element) && (xmlfilereader.Name == "emotion"))
                    {
                        try
                        {
                            rec_time += double.Parse(xmlfilereader.GetAttribute("start"), CultureInfo.InvariantCulture);
                            if (run_1)
                            {
                                starting_time = rec_time;
                                run_1 = false;
                            }
                            Duration = rec_time - starting_time;
                            Sample_count++;
                        }
                        catch (ArgumentNullException e)
                        {
                            Console.WriteLine("Attribute name is null: {0}", e);
                        }
                    }
                    if ((xmlfilereader.NodeType == XmlNodeType.Element) && (xmlfilereader.Name == "category"))
                    {
                        if ((xmlfilereader.GetAttribute("name") == "anger"))
                        {
                            try
                            {
                                string value = xmlfilereader.GetAttribute("value");
                                if (value != string.Empty) Anger += double.Parse(value, CultureInfo.InvariantCulture);
                            }
                            catch (ArgumentNullException e)
                            {
                                Console.WriteLine("Attribute name is null: {0}", e);
                            }
                        }
                        else if ((xmlfilereader.GetAttribute("name") == "contempt"))
                        {
                            try
                            {
                                string value = xmlfilereader.GetAttribute("value");
                                if (value != string.Empty) Contempt += double.Parse(value, CultureInfo.InvariantCulture);
                            }
                            catch (ArgumentNullException e)
                            {
                                Console.WriteLine("Attribute name is null: {0}", e);
                            }
                        }
                        else if ((xmlfilereader.GetAttribute("name") == "disgust"))
                        {
                            try
                            {
                                string value = xmlfilereader.GetAttribute("value");
                                if (value != string.Empty) Disgust += double.Parse(value, CultureInfo.InvariantCulture);
                            }
                            catch (ArgumentNullException e)
                            {
                                Console.WriteLine("Attribute name is null: {0}", e);
                            }
                        }
                        else if ((xmlfilereader.GetAttribute("name") == "fear"))
                        {
                            try
                            {
                                string value = xmlfilereader.GetAttribute("value");
                                if (value != string.Empty) Fear += double.Parse(value, CultureInfo.InvariantCulture);
                            }
                            catch (ArgumentNullException e)
                            {
                                Console.WriteLine("Attribute name is null: {0}", e);
                            }
                        }
                        else if ((xmlfilereader.GetAttribute("name") == "joy"))
                        {
                            try
                            {
                                string value = xmlfilereader.GetAttribute("value");
                                if (value != string.Empty) Joy += double.Parse(value, CultureInfo.InvariantCulture);
                            }
                            catch (ArgumentNullException e)
                            {
                                Console.WriteLine("Attribute name is null: {0}", e);
                            }
                        }
                        else if ((xmlfilereader.GetAttribute("name") == "sadness"))
                        {
                            try
                            {
                                string value = xmlfilereader.GetAttribute("value");
                                if (value != string.Empty) Sadness += double.Parse(value, CultureInfo.InvariantCulture);
                            }
                            catch (ArgumentNullException e)
                            {
                                Console.WriteLine("Attribute name is null: {0}", e);
                            }
                        }
                        else if ((xmlfilereader.GetAttribute("name") == "surprise"))
                        {
                            try
                            {
                                string value = xmlfilereader.GetAttribute("value");
                                if (value != string.Empty) Surprise += double.Parse(value, CultureInfo.InvariantCulture);
                            }
                            catch (ArgumentNullException e)
                            {
                                Console.WriteLine("Attribute name is null: {0}", e);
                            }
                        }
                        else if ((xmlfilereader.GetAttribute("name") == "Engaged"))
                        {
                            try
                            {
                                string value = xmlfilereader.GetAttribute("value");
                                if (value != string.Empty) Engagement += double.Parse(value, CultureInfo.InvariantCulture);
                            }
                            catch (ArgumentNullException e)
                            {
                                Console.WriteLine("Attribute name is null: {0}", e);
                            }
                        }
                    }
                }
                while (!xmlfilereader.EOF);
                #endregion xml2

                Anger = Math.Round((Anger / Sample_count) / 100, 3);
                Contempt = Math.Round((Contempt / Sample_count) / 100, 3);
                Disgust = Math.Round((Disgust / Sample_count) / 100, 3);
                Fear = Math.Round((Fear / Sample_count) / 100, 3);
                Joy = Math.Round((Joy / Sample_count) / 100, 3);
                Sadness = Math.Round((Sadness / Sample_count) / 100, 3);
                Surprise = Math.Round((Surprise / Sample_count) / 100, 3);
                Engagement = Math.Round((Engagement / Sample_count) / 100, 3);

                //Duration = Math.Round(Duration * 1000, 0);
                Duration = Math.Round(Duration, 0);
                
                //convert absolute time to current time:
                var Digi_Date_Time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Start_Time);
                //convert duration from miliseconds to HH:mm:ss
                TimeSpan Digi_Duration = TimeSpan.FromMilliseconds(Duration);

                BehaviourObject behaviourObject = new BehaviourObject();
                behaviourObject.Engaged = Engagement.ToString(CultureInfo.InvariantCulture);
                //behaviourObject.Disengaged = (1 - Engagement).ToString(CultureInfo.InvariantCulture);                
                foreach (var propertyInfo in behaviourObject.GetType().GetProperties())
                {
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        if (propertyInfo.GetValue(behaviourObject, null) == null)
                        {
                            propertyInfo.SetValue(behaviourObject, string.Empty, null);
                        }
                    }
                }
                EmotionObject emotionObject = new EmotionObject();
                emotionObject.Anger = Anger.ToString(CultureInfo.InvariantCulture);
                emotionObject.Contempt = Contempt.ToString(CultureInfo.InvariantCulture);
                emotionObject.Disgust = Disgust.ToString(CultureInfo.InvariantCulture);
                emotionObject.Fear = Fear.ToString(CultureInfo.InvariantCulture);
                emotionObject.Joy = Joy.ToString(CultureInfo.InvariantCulture);
                emotionObject.Sadness = Sadness.ToString(CultureInfo.InvariantCulture);
                emotionObject.Surprise = Surprise.ToString(CultureInfo.InvariantCulture);              
                foreach (var propertyInfo in emotionObject.GetType().GetProperties())
                {
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        if (propertyInfo.GetValue(emotionObject, null) == null)
                        {
                            propertyInfo.SetValue(emotionObject, string.Empty, null);
                        }
                    }
                }
                ObservationObject observationObject = new ObservationObject();
                observationObject.Duration = Digi_Duration.ToString(@"hh\:mm\:ss");
                observationObject.Date_Time = Digi_Date_Time.ToString("yyyy-MM-ddTHH:mm:ss");
                observationObject.Sensory_Component = sensor;
                observationObject.Student = student;
                observationObject.Scenario = "Affective_State_Automatic_Detection_";
                observationObject.Session = new string[1];
                observationObject.Session[0] = session;
                
                RestAPI.ProcessPostingObjects(observationObject, null, behaviourObject, emotionObject);
            }
        }
        public void FileWatcher()
        {
            //https://msdn.microsoft.com/en-us/library/system.io.filesystemeventhandler(v=vs.110).aspx
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = watcherFolder;
            //openFileDialog1.Filter = "Xml|*.xml";
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.*";
            //  Register a handler that gets called when a file is created
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            //  Register a handler that gets called if the 
            //  FileSystemWatcher needs to report an error.
            watcher.Error += new ErrorEventHandler(OnError);
            //  Begin watching.
            watcher.EnableRaisingEvents = true;
        }
        //  This method is called when a file is created, changed, or deleted.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            //  Show that a file has been created, changed, or deleted.
            WatcherChangeTypes wct = e.ChangeType;
            Console.WriteLine("File {0} {1}", e.FullPath, wct.ToString());
            if (!IsFileLocked(new FileInfo(e.FullPath)))
            {
                string filename = Path.GetFileNameWithoutExtension(e.FullPath);
                Analyser(e.FullPath, filename);
            }
        }
        //  This method is called when the FileSystemWatcher detects an error.
        private static void OnError(object source, ErrorEventArgs e)
        {
            //  Show that an error has been detected.
            Console.WriteLine("The FileSystemWatcher has detected an error");
            //  Give more information if the error is due to an internal buffer overflow.
            /*if (e.GetException().GetType() == typeof(System.IO.InternalBufferOverflowException))
            {
                //  This can happen if Windows is reporting many file system events quickly 
                //  and internal buffer of the  FileSystemWatcher is not large enough to handle this
                //  rate of events. The InternalBufferOverflowException error informs the application
                //  that some of the file system events are being lost.
                Console.WriteLine(("The file system watcher experienced an internal buffer overflow: " + e.GetException().Message));
            }*/
        }
        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException err)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                Console.WriteLine("Exception {0}", err);
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Dispose();
            }

            //file is not locked
            return false;
        }
        public static double ConvertToUnixTimestamp(DateTime date)
        {
            //http://stackoverflow.com/questions/3354893/how-can-i-convert-a-datetime-to-the-number-of-seconds-since-1970
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalMilliseconds);
        }
    }
}