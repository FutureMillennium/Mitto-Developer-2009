		// +
          if (arlBuffer.Count > 1)
          {
            double dblResult = 0;
            int i, toRemove = 0, found = 0;
            for (i = 0; i < arlBuffer.Count; i++)
            {
              if (arlBuffer[i].GetType() == typeof(double))
              {
                dblResult += (double)arlBuffer[i];
                if (found == 0)
                  toRemove = i;
                else
                {
                  arlBuffer.RemoveAt(i);
                  i--;
                }
                found++;
              }
              else if (((string)arlBuffer[i]).Substring(0, 1) == "\"" || ((string)arlBuffer[i]).Substring(0, 1) == "'")
              {
                try
                {
                  dblResult += double.Parse(((string)arlBuffer[i]).Substring(1, ((string)arlBuffer[i]).Length - 2));
                  if (found == 0)
                    toRemove = i;
                  else
                  {
                    arlBuffer.RemoveAt(i);
                    i--;
                  }
                  found++;
                }
                catch
                {

                }
              }
            }

            if (found > 1)
            {
              arlBuffer.RemoveAt(toRemove);
              arlBuffer.Add(dblResult);
            }
            else
              arlBuffer.Add(strEval);
          }
          else
            arlBuffer.Add(strEval);

			
			
			
			
			
			
			
			
			
			
			
			
		// print
			for (int i = 0; i < arlBuffer.Count; i++)
            {
              if (arlBuffer[i].GetType() == typeof(double))
              {
                OutputF(arlBuffer[i].ToString(), 4);
                arlBuffer.RemoveAt(i);
                i--;
              }
              else if (((string)arlBuffer[i]).Substring(0, 1) == "\"" || ((string)arlBuffer[i]).Substring(0, 1) == "'")
              {
                OutputF(((string)arlBuffer[i]).Substring(1, ((string)arlBuffer[i]).Length - 2), 4);
                arlBuffer.RemoveAt(i);
                i--;
              }
            }
			
		
		
		
		
		
		
		
		
		
		
		
		// -
		if (arlBuffer.Count > 0)
          {
            double dblResult = 0;
            int i, toRemove = 0, found = 0;
            for (i = 0; i < arlBuffer.Count; i++)
            {
              if (arlBuffer[i].GetType() == typeof(double))
              {
                if (found == 0)
                {
                  dblResult = (double)arlBuffer[i];
                  toRemove = i;
                }
                else
                {
                  dblResult -= (double)arlBuffer[i];
                  arlBuffer.RemoveAt(i);
                  i--;
                }
                found++;
              }
              else if (((string)arlBuffer[i]).Substring(0, 1) == "\"" || ((string)arlBuffer[i]).Substring(0, 1) == "'")
              {
                try
                {
                  if (found == 0)
                  {
                    dblResult = double.Parse(((string)arlBuffer[i]).Substring(1, ((string)arlBuffer[i]).Length - 2));
                    toRemove = i;
                  }
                  else
                  {
                    dblResult -= double.Parse(((string)arlBuffer[i]).Substring(1, ((string)arlBuffer[i]).Length - 2));
                    arlBuffer.RemoveAt(i);
                    i--;
                  }
                  found++;
                }
                catch
                {

                }
              }
            }

            if (found > 0)
            {
              arlBuffer.RemoveAt(toRemove);
              if (found == 1)
                dblResult = -dblResult;
              arlBuffer.Add(dblResult);
            }
            else
              arlBuffer.Add(strEval);
          }
          else
            arlBuffer.Add(strEval);
			
			
			
			
			
			
			
			
			
			
			
			
			
		// *
		double dblResult = 1;
            int i, toRemove = 0, found = 0;
            for (i = 0; i < arlBuffer.Count; i++)
            {
              if (arlBuffer[i].GetType() == typeof(double))
              {
                dblResult *= (double)arlBuffer[i];
                if (found == 0)
                  toRemove = i;
                else
                {
                  arlBuffer.RemoveAt(i);
                  i--;
                }
                found++;
              }
              else if (((string)arlBuffer[i]).Substring(0, 1) == "\"" || ((string)arlBuffer[i]).Substring(0, 1) == "'")
              {
                try
                {
                  dblResult *= double.Parse(((string)arlBuffer[i]).Substring(1, ((string)arlBuffer[i]).Length - 2));
                  if (found == 0)
                    toRemove = i;
                  else
                  {
                    arlBuffer.RemoveAt(i);
                    i--;
                  }
                  found++;
                }
                catch
                {

                }
              }
            }

            if (found > 1)
            {
              arlBuffer.RemoveAt(toRemove);
              arlBuffer.Add(dblResult);
            }
            else
              arlBuffer.Add(strEval);
			  
			  
			  
			  
			  
			  
			  
			  
			  
			  
			  
			  
			  
			  
			  
		// /
		double dblResult = 0, dblTmp;
            int i, toRemove = 0, found = 0;
            for (i = 0; i < arlBuffer.Count; i++)
            {
              if (arlBuffer[i].GetType() == typeof(double))
              {
                dblTmp = (double)arlBuffer[i];
                if ((double)arlBuffer[i] != 0)
                {
                  if (found == 0)
                  {
                    dblResult = dblTmp;
                    toRemove = i;
                  }
                  else
                  {
                    dblResult /= dblTmp;
                    arlBuffer.RemoveAt(i);
                    i--;
                  }
                  found++;
                }
              }
              else if (((string)arlBuffer[i]).Substring(0, 1) == "\"" || ((string)arlBuffer[i]).Substring(0, 1) == "'")
              {
                try
                {
                  dblTmp = double.Parse(((string)arlBuffer[i]).Substring(1, ((string)arlBuffer[i]).Length - 2));
                  if (dblTmp != 0)
                  {
                    if (found == 0)
                    {
                      dblResult = dblTmp;
                      toRemove = i;
                    }
                    else
                    {
                      dblResult /= dblTmp;
                      arlBuffer.RemoveAt(i);
                      i--;
                    }
                    found++;
                  }
                }
                catch
                {

                }
              }
            }

            if (found > 1)
            {
              arlBuffer.RemoveAt(toRemove);
              arlBuffer.Add(dblResult);
            }
            else
              arlBuffer.Add(strEval);
			  
			  
			  
			  
			  
			  
			  
			  
			  
			  
			  
			  
			  
			  
			  
			  
			  
		// Check for envelopes (
		pos1 = at + 1;
            togo = 1;
            togo2 = 0;
            next--;
            while (true)
            {
              pos2 = txtEval.IndexOf(")", pos1);
              if (pos2 == -1)
              {
                thiscmd = txtEval.Substring(atorig, next - atorig).Trim();
                break;
              }
              togo2++;
              togo += SubstrCount(txtEval, "(", pos1, pos2);
              if (togo - togo2 <= 0)
              {
                next = pos2 + 1;
                thiscmd = txtEval.Substring(atorig, next - atorig).Trim();
                break;
              }
              else
              {
                pos1 = pos2 + 1;
              }
            }*/
            //thiscmd = MittoParseOut(tmp, p, "(", ")", at, at2, next);