		ArrayList MittoBuffer = new ArrayList();
        //ArrayList MittoVariables = new ArrayList(); // Not used yet
        //ArrayList MittoLevels = new ArrayList(); // Not used yet
        //ArrayList MittoFunctions = new ArrayList(); // Not used yet
        ArrayList MittoQueue = new ArrayList();

        private void MittoOutput(string p, Color clrColor, RichTextBox output)
        {
            output.Select(output.Text.Length, 0);
            output.SelectionColor = clrColor;
            output.SelectedText = p;
            output.ScrollToCaret();
        }

        public void MittoEval(string tmp, RichTextBox output, int level, ArrayList theBuffer, ArrayList theQueue)
        {
            double tmp2 = 0;
            bool isNumber;
            ArrayList MittoTmp = new ArrayList();
            //tmp = tmp.Trim();

            // Ignore it if it's a comment #, /* */, //
            //if (tmp.Substring(0, 1) != "#" && !(tmp.Length >= 2 && ((tmp.Substring(0, 2) == "/*" && tmp.Substring(tmp.Length - 2) == "*/") || tmp.Substring(0, 2) == "//")))
            //{

            if (tmp.Length > 0)
            {

                // If it's a string, add it to the buffer
                if (tmp.Substring(0, 1) == "\"" && tmp.Substring(tmp.Length - 1, 1) == "\"")
                {
                    //TODO: Check it for variables
                    theBuffer.Add(tmp);
                }
                else
                {
                    // Convert the letter to a number to see if it is one
                    try
                    {
                        tmp2 = Convert.ToDouble(tmp);
                        isNumber = true;
                    }
                    catch (FormatException) // e
                    {
                        //MittoOutput("> (Letter is not a number.)\n", clrWarning, output);
                        isNumber = false;
                    }
                    catch (Exception) // e
                    {
                        // —\(°_o)/—
                        isNumber = false;
                    }

                    // If it's a number, add it to the buffer
                    if (isNumber == true)
                    {
                        theBuffer.Add(tmp2);
                    }
                    else
                    {
                        //It's not, so let's eval it
                        switch (tmp.ToLower())
                        {
                            // Check the queue
                            case ";":
                            case "check":
                                if (MittoQueue.Count > 0)
                                {
                                    //MittoQueue.Reverse();
                                    foreach (object i in MittoQueue)
                                        MittoTmp.Add(i);
                                    MittoQueue.Clear();

                                    //for (int i = 0; i < MittoQueue.Count; i++)
                                    foreach (string i in MittoTmp)
                                    {
                                        MittoEval(i, output, level, theBuffer, theQueue);
                                        //MittoTmp.Add(i);
                                    }

                                    //MittoQueue.Clear();
                                    //foreach (object i in MittoTmp)
                                    //MittoQueue.Remove(i);
                                    //MittoTmp.Clear();
                                }

                                break;

                            // Clear the buffer, write it all out
                            case "clearbuffer":
                                if (MittoQueue.Count > 0)
                                {
                                    MittoQueue.Reverse();
                                    foreach (string i in MittoQueue)
                                        MittoOutput("@ " + i + "\n", clrQueue, output);
                                    MittoQueue.Clear();
                                }

                                if (theBuffer.Count > 0)
                                {
                                    foreach (object i in theBuffer)
                                        MittoOutput("# " + i + "\n", clrBuffer, output);
                                    theBuffer.Clear();
                                }
                                break;

                            case "endbuffer":
                                if (MittoQueue.Count > 0)
                                    MittoEval("check", output, level, theBuffer, theQueue);
                                if (theBuffer.Count > 0)
                                    MittoEval("clearbuffer", output, level, theBuffer, theQueue);
                                break;

                            // Addition
                            case "+":
                                if (theBuffer.Count >= 2)
                                {
                                    foreach (object i in theBuffer)
                                    {
                                        try
                                        {
                                            tmp2 += Convert.ToDouble(i);
                                            //MittoBuffer.Remove(i);
                                            MittoTmp.Add(i);
                                        }
                                        catch (Exception) // e
                                        {
                                            // —\(°_o)/—
                                        }
                                    }

                                    if (MittoTmp.Count < 2)
                                    {
                                        //MittoTmp.Clear();
                                        MittoQueue.Add(tmp);
                                    }
                                    else
                                    {
                                        foreach (object i in MittoTmp)
                                            theBuffer.Remove(i);
                                        //MittoTmp.Clear();

                                        //MittoBuffer.Clear();
                                        theBuffer.Add(tmp2);
                                    }
                                }
                                else
                                {
                                    MittoQueue.Add(tmp);
                                }
                                break;

                            // Substraction
                            case "-":
                                if (theBuffer.Count >= 2)
                                {
                                    foreach (object i in theBuffer)
                                    {
                                        try
                                        {
                                            if (MittoTmp.Count == 0)
                                                tmp2 = Convert.ToDouble(i);
                                            else
                                                tmp2 -= Convert.ToDouble(i);
                                            MittoTmp.Add(i);
                                        }
                                        catch (Exception)
                                        {
                                            // —\(°_o)/—
                                        }
                                    }

                                    if (MittoTmp.Count < 2)
                                    {
                                        //MittoTmp.Clear();
                                        MittoQueue.Add(tmp);
                                    }
                                    else
                                    {
                                        foreach (object i in MittoTmp)
                                            theBuffer.Remove(i);
                                        //MittoTmp.Clear();

                                        theBuffer.Add(tmp2);
                                    }
                                }
                                else
                                {
                                    MittoQueue.Add(tmp);
                                }
                                break;

                            // Multiplication
                            case "*":
                                if (theBuffer.Count >= 2)
                                {
                                    tmp2 = 1;
                                    foreach (object i in theBuffer)
                                    {
                                        try
                                        {
                                            tmp2 = tmp2 * Convert.ToDouble(i);
                                            MittoTmp.Add(i);
                                        }
                                        catch (Exception)
                                        {
                                            // —\(°_o)/—
                                        }
                                    }

                                    if (MittoTmp.Count < 2)
                                    {
                                        //MittoTmp.Clear();
                                        MittoQueue.Add(tmp);
                                    }
                                    else
                                    {
                                        foreach (object i in MittoTmp)
                                            theBuffer.Remove(i);
                                        //MittoTmp.Clear();

                                        theBuffer.Add(tmp2);
                                    }
                                }
                                else
                                {
                                    MittoQueue.Add(tmp);
                                }
                                break;

                            // Division
                            case "/":
                                if (theBuffer.Count >= 2)
                                {
                                    tmp2 = 0;
                                    foreach (object i in theBuffer)
                                    {
                                        try
                                        {
                                            if (Convert.ToDouble(i) != 0)
                                                if (tmp2 == 0)
                                                    tmp2 = Convert.ToDouble(i);
                                                else
                                                    tmp2 = tmp2 / Convert.ToDouble(i);

                                            MittoTmp.Add(i);
                                        }
                                        catch (Exception)
                                        {
                                            // —\(°_o)/—
                                        }
                                    }

                                    if (MittoTmp.Count < 2)
                                    {
                                        //MittoTmp.Clear();
                                        MittoQueue.Add(tmp);
                                    }
                                    else
                                    {
                                        foreach (object i in MittoTmp)
                                            theBuffer.Remove(i);
                                        //MittoTmp.Clear();

                                        theBuffer.Add(tmp2);
                                    }
                                }
                                else
                                {
                                    MittoQueue.Add(tmp);
                                }
                                break;

                            // Printing out
                            case "echo":
                            case "print":
                            case "display":
                                if (theBuffer.Count >= 1)
                                {
                                    tmp = theBuffer[theBuffer.Count - 1].ToString();
                                    if (tmp.Substring(0, 1) == "\"" && tmp.Substring(tmp.Length - 1, 1) == "\"")
                                    {
                                        tmp = tmp.Substring(1, tmp.Length - 2);
                                    }
                                    MittoOutput(Regex.Unescape(tmp) + "\n", clrOutput, output);
                                    theBuffer.RemoveAt(theBuffer.Count - 1);
                                }
                                else
                                {
                                    MittoQueue.Add(tmp);
                                }
                                break;

                            // Well, add it to the buffer anyway!
                            default:
                                //TODO: Functions
                                //TODO: Variables
                                theBuffer.Add(tmp);
                                break;
                        }
                    }
                }

                //MittoOutput("> " + tmp + "\n", clrBuffer, output);
                //}
            }
        }

        public int SubstrCount(string p, string substr, int start, int stop)
        {
            int tmp = 0;
            if (stop > start && start <= p.Length && stop <= p.Length)
            {
                int next = p.IndexOf(substr, start, stop - start);

                while (next > -1)
                {
                    tmp++;
                    next = p.IndexOf(substr, next + 1, stop - next + 1);
                }
            }

            return tmp;
        }

        public int FindNextWhite(string p, int start)
        {
            int tmp1 = p.IndexOf(" ", start);
            int tmp2 = p.IndexOf("\t", start);

            if (tmp1 == -1)
                tmp1 = tmp2;
            else
                if (tmp2 != -1)
                    tmp1 = Math.Min(tmp1, tmp2);

            tmp2 = p.IndexOf("\n", start);

            if (tmp1 == -1)
                tmp1 = tmp2;
            else
                if (tmp2 != -1)
                    tmp1 = Math.Min(tmp1, tmp2);

            return tmp1;
        }

        /*
        private string MittoParseOut(string tmp, string p, string p_3, string p_4, int at, int at2, int next)
        {
            int p1, p2;
            int togo, togo2;

            p1 = at + 1;
            /*
            togo = 0;
            while (imdone == false)
            {
                p2 = tmp.IndexOf(")", p1);
                if (p2 > -1)
                {
                    p1 = tmp.IndexOf("(", p1);
                    if ((p1 > p2 || p1 == -1) && togo <= 0)
                    {
                        thiscmd = p.Substring(at, p2 - at + 1);
                        next = p2;
                        imdone = true;
                    }
                    else
                    {
                        if (p1 > -1)
                            togo += SubstrCount(tmp, "(", p1, p2);
                        togo--;
                        p1 = p2 + 1;
                    }
                }
                else
                {
                    imdone = true;
                }
            }
            */ /*
            togo = 1;
            togo2 = 0;
            while (true)
            {
                p2 = tmp.IndexOf(p_4, p1);
                if (p2 == -1)
                {
                    return p.Substring(at2, next - at2).Trim();
                    //break;
                }
                togo2++;
                togo += SubstrCount(tmp, p_3, p1, p2);
                if (togo - togo2 <= 0)
                {
                    next = p2 + 1;
                    return p.Substring(at2, next - at2).Trim();
                    //break;
                }
                else
                {
                    p1 = p2 + 1;
                }
            }
        }
        */

        public ArrayList MittoParser(string p)
        {
            ArrayList res = new ArrayList();
            p = p.Trim();
            if (p.Length > 0)
            {
                string tmp = p.ToLower();

                int next;
                int at = 0, at2, at3;
                int p1, p2;
                int togo, togo2;
                int plen = p.Length;
                string thiscmd;
                //bool imdone = false;

                //next = FindNextWhite(tmp, at);
                while (at <= plen)
                {
                    next = FindNextWhite(tmp, at);
                    if (next == -1)
                        next = plen;

                    thiscmd = p.Substring(at, next - at).Trim();
                    next++;
                    if (thiscmd.Length > 0)
                    {
                        at2 = at;
                        at3 = 0;

                        if (thiscmd.Length > 1 && (thiscmd.Substring(0, 1) == "#" || thiscmd.Substring(0, 1) == "$"))
                        {
                            at++; // Remember that "at" changes, not "at2", so at2 is the original
                            at3 = 1;
                        }

                        // Check for full line comments // \n
                        if (thiscmd.Length - at3 >= 2 && thiscmd.Substring(at3, 2) == "//")
                        {
                            p1 = tmp.IndexOf("\n", at);
                            if (p1 == -1)
                            {
                                thiscmd = p.Substring(at2, plen - at2);
                                next = plen;
                            }
                            else
                            {
                                thiscmd = p.Substring(at2, p1 - at2);
                                next = p1 + 1;
                            }
                        }
                        // Check for block comments /* */
                        else if (thiscmd.Length - at3 >= 2 && thiscmd.Substring(at3, 2) == "/*")
                        {
                            p1 = tmp.IndexOf("*/", at);
                            if (p1 == -1)
                            {
                                next = plen;
                            }
                            else
                            {
                                next = p1 + 2;
                            }
                            thiscmd = p.Substring(at2, next - at2);
                        }
                        // Check for texts/strings " "
                        else if (thiscmd.Length - at3 >= 2 && thiscmd.Substring(at3, 1) == "\"")
                        {
                            //while (true)
                            //{
                                p1 = tmp.IndexOf("\"", at + 1);
                                if (p1 == -1)
                                {
                                    next = plen;
                                }
                                else
                                {
                                    next = p1 + 1;
                                }
                                thiscmd = p.Substring(at2, next - at2);
                                //break;
                            //}
                        }
                        // Check for envelopes ( )
                        else if (thiscmd.Substring(at3, 1) == "(")
                        {
                            p1 = at + 1;
                            togo = 1;
                            togo2 = 0;
                            next--;
                            while (true)
                            {
                                p2 = tmp.IndexOf(")", p1);
                                if (p2 == -1)
                                {
                                    thiscmd = p.Substring(at2, next - at2).Trim();
                                    break;
                                }
                                togo2++;
                                togo += SubstrCount(tmp, "(", p1, p2);
                                if (togo - togo2 <= 0)
                                {
                                    next = p2 + 1;
                                    thiscmd = p.Substring(at2, next - at2).Trim();
                                    break;
                                }
                                else
                                {
                                    p1 = p2 + 1;
                                }
                            }
                            //thiscmd = MittoParseOut(tmp, p, "(", ")", at, at2, next);
                        }
                        res.Add(thiscmd);
                    }
                    at = next;
                }
            }
            return res;
        }

        public void MittoParse(string p, RichTextBox output, int level, ArrayList parentBuffer, ArrayList theBuffer, ArrayList parentQueue, ArrayList theQueue)
        {
            ArrayList sass = MittoParser(p);
            /*foreach (object i in sass)
                MittoOutput("# " + i + "\n", clrBuffer, output);
            return;*/

            string tmp;
            //double tmp2 = 0;
            //bool isNumber;
            //ArrayList MittoTmp = new ArrayList();

            // Don't eval if it's empty
            if (p.Length > 0)
            {
                // If it's an envelope ( ), we want to parse it
                //if (p.Substring(0, 1) == "(" && p.Substring(p.Length - 1, 1) == ")")
                    //p = p.Substring(1, p.Length - 2);

                //p = p.Trim();
                //Regex r = new Regex("(.*) ");

                // Match envelopes, strings and other letters
                ArrayList sa = MittoParser(p);
                //MatchCollection sa = Regex.Matches(p, @"(/\*.*\*/)|(//.*$)|([(].*[)])|(#?""[^"".]*"")|([^ ^\n.\.]*)", RegexOptions.Multiline);

                // OLD: If there's just one letter, eval it, otherwise parse it for evaluation

                // Eval each letter
                for (int i = 0; i < sa.Count; i++)
                {
                    tmp = sa[i].ToString().Trim();
                    // If it's not empty
                    if (tmp != "")
                    {
                        if (tmp.Substring(0, 1) != "#" && !(tmp.Length >= 2 && ((tmp.Substring(0, 2) == "/*" && tmp.Substring(tmp.Length - 2) == "*/") || tmp.Substring(0, 2) == "//")))
                        {
                            if (tmp.Substring(0, 1) == "(" && tmp.Substring(tmp.Length - 1, 1) == ")")
                            {
                                tmp = tmp.Substring(1, tmp.Length - 2);
                                //int b = theBuffer.Add(new ArrayList());
                                //int q = theQueue.Add(new ArrayList());
                                //MittoParse(tmp, output, level + 1, theBuffer, theBuffer[b], theQueue, theQueue[q]);
                                MittoParse(tmp, output, level + 1, theBuffer, new ArrayList(), theQueue, new ArrayList());
                            }
                            else
                            {
                                MittoEval(tmp, output, level + 1, theBuffer, theQueue);
                            }
                        }
                        /*output.Select(output.Text.Length, 0);
                        output.SelectionColor = clrBuffer;
                        output.SelectedText = "> " + tmp + "\n";
                        output.ScrollToCaret();*/
                    }
                }
                MittoEval("check", output, level + 1, theBuffer, theQueue);
                if (parentBuffer != null)
                {
                    foreach (object i in theBuffer)
                        parentBuffer.Add(i);
                    //parentBuffer.Remove(theBuffer);

                    foreach (object i in theQueue)
                        parentQueue.Add(i);
                    //parentQueue.Remove(theQueue);
                }
                
            }

            if (level == 0)
                MittoEval("clearbuffer", output, -1, theBuffer, theQueue);

            Application.DoEvents();
        }