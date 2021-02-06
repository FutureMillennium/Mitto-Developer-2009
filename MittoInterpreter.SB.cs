using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

namespace MittoDeveloper
{
  class MittoInterpreter
  {
    public delegate void tOutput(string p, int type);
    public tOutput OutputF;
    public bool blPrintParser = false;
    public Random random = new Random();

    public List<object> arlBuffer = new List<object>();
    public List<string> lstVariables = new List<string>();
    public List<object> arlVariableValues = new List<object>();

    /*public static int SubstrCount(string p, string substr, int start, int stop)
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
    }*/

    public static int FindNextWhite(string p, int start)
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

    public static int FindPrevWhite(string p, int start)
    {
      int tmp1 = p.LastIndexOf(' ', start);
      int tmp2 = p.LastIndexOf('\t', start);

      if (tmp1 == -1)
        tmp1 = tmp2;
      else
        if (tmp2 != -1)
          tmp1 = Math.Max(tmp1, tmp2);

      tmp2 = p.LastIndexOf('\n', start);

      if (tmp1 == -1)
        tmp1 = tmp2;
      else
        if (tmp2 != -1)
          tmp1 = Math.Max(tmp1, tmp2);

      return tmp1;
    }

    public void PrintSource(List<object> arlSource, int intLevel)
    {
      for (int i = 0; i < arlSource.Count; i++)
      {
        if (arlSource[i].GetType() == typeof(List<object>))
        {
          OutputF("(\n", 3);
          PrintSource((List<object>)arlSource[i], intLevel + 1);
        }
        else
          OutputF(arlSource[i].ToString().PadLeft(intLevel * 2) + "\n", 2);
      }

      if (intLevel > 0)
        OutputF(")\n", 3);
    }

    public bool IsString(object s)
    {
      if (s.GetType() == typeof(string) && (((string)s).Substring(0, 1) == "\"" || ((string)s).Substring(0, 1) == "'"))
        return true;
      else
        return false;
    }

    public bool IsSymbol(object s)
    {
      if (s.GetType() == typeof(string) && ((string)s).Substring(0, 1) != "\"" && ((string)s).Substring(0, 1) != "'" && ((string)s) != "false" && ((string)s) != "true")
        return true;
      else
        return false;
    }

    // Buffer check
    //--------------------------------------------------------------------------------------------------------------------------------
    public void BufferCheck(int intMyBufferPos, int intParBufferPos, int intMyPos, List<object> arlMySource, int intParPos, List<object> arlParSource, ref int intMySkip, ref int intParSkip, ref int intVariablePop)
    {
      int i, max;

      /*for (i = arlBuffer.Count - 1; i >= 0; i--)
      {
        if (i < arlBuffer.Count)
        {*/
      max = arlBuffer.Count - intMyBufferPos;

      for (i = intMyBufferPos; i < max; i++)
      {
          if (IsSymbol(arlBuffer[i]))
          {
            string strCmd = (string)arlBuffer[i];
            arlBuffer.RemoveAt(i);
            SymbolEval(strCmd, intMyBufferPos, intParBufferPos, intMyPos, arlMySource, intParPos, arlParSource, ref intMySkip, ref intParSkip, ref intVariablePop);
            i--;
            max--;
          }
        //}
      }
    }

    // Get next symbol
    //--------------------------------------------------------------------------------------------------------------------------------
    public object GetNextSymbol(int intNext, int intPos, List<object> arlSource)
    {
      if (intPos + intNext >= arlSource.Count)
        return null;
      else
        return arlSource[intPos + intNext];
    }

    // Take a number of strings from the buffer
    //--------------------------------------------------------------------------------------------------------------------------------
    public List<string> TakeStrings(int intCount, int intMin, int intMyBufferPos, int intMyBufferEnd, bool blStrict)
    {
      List<string> lstResult = new List<string>();

      if (intMyBufferEnd - intMyBufferPos >= intMin)
      {
        int i, toRemove = 0, found = 0;
        for (i = intMyBufferEnd - 1; i >= intMyBufferPos; i--)
        {
          if (blStrict == false && arlBuffer[i].GetType() == typeof(double))
          {
            lstResult.Insert(0, arlBuffer[i].ToString());
            if (found == 0)
              toRemove = i;
            else
            {
              arlBuffer.RemoveAt(i);
              toRemove--;
            }
            found++;
            if (intCount == found)
              break;
          }
          else if (((string)arlBuffer[i]).Substring(0, 1) == "\"" || ((string)arlBuffer[i]).Substring(0, 1) == "'")
          {
            lstResult.Insert(0, ((string)arlBuffer[i]).Substring(1, ((string)arlBuffer[i]).Length - 2));
            if (found == 0)
              toRemove = i;
            else
            {
              arlBuffer.RemoveAt(i);
              toRemove--;
            }
            found++;
            if (intCount == found)
              break;
          }
          else if (blStrict == false && ((string)arlBuffer[i] == "true" || (string)arlBuffer[i] == "false")) {
            lstResult.Insert(0, (string)arlBuffer[i]);
            if (found == 0)
              toRemove = i;
            else
            {
              arlBuffer.RemoveAt(i);
              toRemove--;
            }
            found++;
            if (intCount == found)
              break;
          }
        }

        if (found >= intMin)
          arlBuffer.RemoveAt(toRemove);
        else
          lstResult.Clear();
      }

      return lstResult;
    }

    // Take a number of numbers from the buffer
    //--------------------------------------------------------------------------------------------------------------------------------
    public List<double> TakeNumbers(int intCount, int intMin, int intMyBufferPos, int intMyBufferEnd, bool blStrict, bool blNonZero)
    {
      List<double> lstResult = new List<double>();

      if (intMyBufferEnd - intMyBufferPos >= intMin)
      {
        int i, toRemove = 0, found = 0;
        double dblTmp;
        for (i = intMyBufferEnd - 1; i >= intMyBufferPos; i--)
        {
          if (arlBuffer[i].GetType() == typeof(double))
          {
            if (blNonZero == false || (blNonZero && (double)arlBuffer[i] != 0))
            {
              lstResult.Insert(0, (double)arlBuffer[i]);
              if (found == 0)
                toRemove = i;
              else
              {
                arlBuffer.RemoveAt(i);
                toRemove--;
              }
              found++;
              if (intCount == found)
                break;
            }
          }
          else if (blStrict == false && (((string)arlBuffer[i]).Substring(0, 1) == "\"" || ((string)arlBuffer[i]).Substring(0, 1) == "'"))
          {
            if (double.TryParse(((string)arlBuffer[i]).Substring(1, ((string)arlBuffer[i]).Length - 2), out dblTmp))
            {
              lstResult.Insert(0, dblTmp);
              if (found == 0)
                toRemove = i;
              else
              {
                arlBuffer.RemoveAt(i);
                toRemove--;
              }
              found++;
              if (intCount == found)
                break;
            }
          }
        }

        if (found >= intMin)
          arlBuffer.RemoveAt(toRemove);
        else
          lstResult.Clear();
      }

      return lstResult;
    }

    // Take a number of any objects from the buffer
    //--------------------------------------------------------------------------------------------------------------------------------
    public List<object> TakeAny(int intCount, int intMin, int intMyBufferPos, int intMyBufferEnd)
    {
      List<object> lstResult = new List<object>();

      if (intMyBufferEnd - intMyBufferPos >= intMin)
      {
        int i, found = 0;
        for (i = intMyBufferEnd - 1; i >= intMyBufferPos; i--)
        {
          lstResult.Insert(0, arlBuffer[i]);
          arlBuffer.RemoveAt(i);
          found++;
          if (intCount == found)
            break;
        }
      }

      return lstResult;
    }

    // Eval a letter, choose what to do with it
    //--------------------------------------------------------------------------------------------------------------------------------
    public void LetterEval(object objEval, int intMyBufferPos, int intParBufferPos, int intMyPos, List<object> arlMySource, int intParPos, List<object> arlParSource, ref int intMySkip, ref int intParSkip, ref int intVariablePop)
    {
      if (objEval == null)
        return;
      else if (objEval.GetType() == typeof(List<object>))
        MittoEval((List<object>)objEval, intMyBufferPos, intMyPos, arlMySource, ref intMySkip);
      else if (objEval.GetType() == typeof(double))
        arlBuffer.Add(objEval);
      else if (objEval.GetType() == typeof(MittoClass))
        arlBuffer.Add(objEval);
      else if (IsString(objEval))
        arlBuffer.Add(objEval);
      else
        SymbolEval((string)objEval, intMyBufferPos, intParBufferPos, intMyPos, arlMySource, intParPos, arlParSource, ref intMySkip, ref intParSkip, ref intVariablePop);
    }

    // The logical value of a letter
    //--------------------------------------------------------------------------------------------------------------------------------
    public bool GetLogicalValue(object objEval)
    {
      if (objEval == null)
      {
        return false;
      }
      if (objEval.GetType() == typeof(double))
      {
        if ((double)objEval == 0)
          return false;
        else
          return true;
      }
      else if (objEval.GetType() == typeof(List<object>))
      {
        if (((List<object>)objEval).Count == 0)
          return false;
        else
          return true;
      }
      else if (objEval.GetType() == typeof(MittoClass))
      {
        return true;
      }
      else if (IsString(objEval))
      {
        double dblTmp;
        if (double.TryParse((string)objEval, out dblTmp))
        {
          if (dblTmp == 0)
            return false;
          else
            return true;
        }
        else
        {
          if ((string)objEval == "")
            return false;
          else
            return true;
        }
      }
      else
      {
        if ((string)objEval == "false")
          return false;
        else
          return true;
      }

    }

    public void ClearBuffer(int intMyBufferPos)
    {
      if (intMyBufferPos <= arlBuffer.Count)
      {
        arlBuffer.RemoveRange(intMyBufferPos, arlBuffer.Count - intMyBufferPos);
      }
    }

    // Symbol eval
    //--------------------------------------------------------------------------------------------------------------------------------
    public void SymbolEval(string strEval, int intMyBufferPos, int intParBufferPos, int intMyPos, List<object> arlMySource, int intParPos, List<object> arlParSource, ref int intMySkip, ref int intParSkip, ref int intVariablePop)
    {
      string strCmd = strEval.ToLower();
      int intVarIndex = lstVariables.IndexOf(strCmd);
      if (intVarIndex != -1)
      {
        LetterEval(arlVariableValues[intVarIndex], intMyBufferPos, intParBufferPos, intMyPos, arlMySource, intParPos, arlParSource, ref intMySkip, ref intParSkip, ref intVariablePop);
        /*if (arlVariableValues[intVarIndex].GetType() == typeof(List<object>))
          MittoEval((List<object>)arlVariableValues[intVarIndex], arlBuffer, intParPos, arlParentBuffer == null ? arlParSource : arlBuffer, ref intParSkip);
        else if (arlVariableValues[intVarIndex].GetType() == typeof(double))
          arlBuffer.Add(arlVariableValues[intVarIndex]);
        else if (((string)arlVariableValues[intVarIndex]).Substring(0, 1) == "\"" || ((string)arlVariableValues[intVarIndex]).Substring(0, 1) == "'")
          arlBuffer.Add(arlVariableValues[intVarIndex]);
        else
          SymbolEval((string)arlVariableValues[intVarIndex], arlBuffer, arlParentBuffer, intMyPos, arlMySource, intParPos, arlParSource, ref intParSkip, ref intMySkip);*/
      }
      else
      {
        switch (strCmd)
        {
          //----------------------------------------------------------------
          case ";":
            BufferCheck(intMyBufferPos, intParBufferPos, intMyPos, arlMySource, intParPos, arlParSource, ref intParSkip, ref intMySkip, ref intVariablePop);
            break;
          //----------------------------------------------------------------
          case "end":
            BufferCheck(intMyBufferPos, intParBufferPos, intMyPos, arlMySource, intParPos, arlParSource, ref intParSkip, ref intMySkip, ref intVariablePop);
            PrintSource(arlBuffer, 0);
            ClearBuffer(intMyBufferPos);
            break;
          //----------------------------------------------------------------
          case "clear":
            ClearBuffer(intMyBufferPos);
            break;
          //----------------------------------------------------------------
          case "unsetall":
            lstVariables.Clear();
            arlVariableValues.Clear();
            break;
          //----------------------------------------------------------------
          case "unset":
            {
              object objVar = GetNextSymbol(1, intMyPos, arlMySource);
              if (objVar != null)
              {
                intVarIndex = lstVariables.IndexOf((string)objVar);
                if (intVarIndex != -1)
                {
                  lstVariables.RemoveAt(intVarIndex);
                  arlVariableValues.RemoveAt(intVarIndex);
                  intMySkip = 1;
                }
              }
            }
            break;
          //----------------------------------------------------------------
          case "=":
            {
              object objVar = GetNextSymbol(1, intMyPos, arlMySource);
              object objValue;
              if (arlBuffer.Count - intMyBufferPos > 0)
              {
                objValue = arlBuffer[arlBuffer.Count - 1];
                arlBuffer.RemoveAt(arlBuffer.Count - 1);
              }
              else
              {
                objValue = GetNextSymbol(2, intMyPos, arlMySource);
                intMySkip++;
              }

              if (objVar != null && objValue != null)
              {
                intVarIndex = lstVariables.IndexOf((string)objVar);
                if (intVarIndex == -1)
                {
                  lstVariables.Add((string)objVar);
                  arlVariableValues.Add(objValue);
                }
                else
                {
                  arlVariableValues[intVarIndex] = objValue;
                }
                intMySkip++;
              }
            }
            break;
          //----------------------------------------------------------------
          case "if":
            {
              object objTest;
              int intShift = 0;

              if (arlBuffer.Count - intMyBufferPos == 0)
              {
                LetterEval(GetNextSymbol(1, intMyPos, arlMySource), intMyBufferPos, intParBufferPos, intMyPos + 1, arlMySource, intParPos, arlParSource, ref intMySkip, ref intParSkip, ref intVariablePop);
                intShift = 1;
              }
              objTest = arlBuffer[arlBuffer.Count - 1];
              arlBuffer.RemoveAt(arlBuffer.Count - 1);

              object objElse = GetNextSymbol(2 + intShift, intMyPos, arlMySource);

              if (GetLogicalValue(objTest))
              {
                LetterEval(GetNextSymbol(1 + intShift, intMyPos, arlMySource), intMyBufferPos, intParBufferPos, intMyPos + 1 + intShift, arlMySource, intParPos, arlParSource, ref intMySkip, ref intParSkip, ref intVariablePop);

                if (objElse != null && objElse.GetType() == typeof(string) && (string)objElse == "else")
                {
                  intShift += 2;
                }
              }
              else
              {
                if (objElse != null && (string)objElse == "else")
                {
                  LetterEval(GetNextSymbol(3 + intShift, intMyPos, arlMySource), intMyBufferPos, intParBufferPos, intMyPos + 3 + intShift, arlMySource, intParPos, arlParSource, ref intMySkip, ref intParSkip, ref intVariablePop);
                  intShift += 2;
                }
              }

              intMySkip = 1 + intShift;
            }
            break;
          //----------------------------------------------------------------
          case "while":
            {
              object objCond = GetNextSymbol(1, intMyPos, arlMySource);
              object objTest;
              object objDo = GetNextSymbol(2, intMyPos, arlMySource);

              if (objCond != null && objDo != null)
              {
                LetterEval(objCond, intMyBufferPos, intParBufferPos, intMyPos + 1, arlMySource, intParPos, arlParSource, ref intMySkip, ref intParSkip, ref intVariablePop);
                if (arlBuffer.Count - intMyBufferPos > 0)
                {
                  objTest = arlBuffer[arlBuffer.Count - 1];
                  arlBuffer.RemoveAt(arlBuffer.Count - 1);
                }
                else
                  objTest = null;

                while (GetLogicalValue(objTest))
                {
                  LetterEval(objDo, intMyBufferPos, intParBufferPos, intMyPos + 2, arlMySource, intParPos, arlParSource, ref intMySkip, ref intParSkip, ref intVariablePop);

                  LetterEval(objCond, intMyBufferPos, intParBufferPos, intMyPos + 1, arlMySource, intParPos, arlParSource, ref intMySkip, ref intParSkip, ref intVariablePop);
                  if (arlBuffer.Count - intMyBufferPos > 0)
                  {
                    objTest = arlBuffer[arlBuffer.Count - 1];
                    arlBuffer.RemoveAt(arlBuffer.Count - 1);
                  }
                  else
                    objTest = null;
                }

                intMySkip = 2;
              }
            }
            break;
          //----------------------------------------------------------------
          case "until":
            {
              object objCond = GetNextSymbol(1, intMyPos, arlMySource);
              object objTest;
              object objDo = GetNextSymbol(2, intMyPos, arlMySource);

              if (objCond != null && objDo != null)
              {
                LetterEval(objCond, intMyBufferPos, intParBufferPos, intMyPos + 1, arlMySource, intParPos, arlParSource, ref intMySkip, ref intParSkip, ref intVariablePop);
                if (arlBuffer.Count - intMyBufferPos > 0)
                {
                  objTest = arlBuffer[arlBuffer.Count - 1];
                  arlBuffer.RemoveAt(arlBuffer.Count - 1);
                }
                else
                  objTest = null;

                while (!GetLogicalValue(objTest))
                {
                  LetterEval(objDo, intMyBufferPos, intParBufferPos, intMyPos + 2, arlMySource, intParPos, arlParSource, ref intMySkip, ref intParSkip, ref intVariablePop);

                  LetterEval(objCond, intMyBufferPos, intParBufferPos, intMyPos + 1, arlMySource, intParPos, arlParSource, ref intMySkip, ref intParSkip, ref intVariablePop);
                  if (arlBuffer.Count - intMyBufferPos > 0)
                  {
                    objTest = arlBuffer[arlBuffer.Count - 1];
                    arlBuffer.RemoveAt(arlBuffer.Count - 1);
                  }
                  else
                    objTest = null;
                }

                intMySkip = 2;
              }
            }
            break;
          //----------------------------------------------------------------
          case "!":
            if (arlBuffer.Count - intMyBufferPos > 0)
            {
              List<object> lstTmp = TakeAny(-1, 1, intMyBufferPos, arlBuffer.Count);

              for (int i = 0; i < lstTmp.Count; i++)
              {
                if (GetLogicalValue(lstTmp[i]))
                  arlBuffer.Add("false");
                else
                  arlBuffer.Add("true");
              }
            }
            else
              arlBuffer.Add(strEval);
            break;
          //----------------------------------------------------------------
          case "<":
            if (arlBuffer.Count - intMyBufferPos > 1)
            {
              List<double> lstDblTmp = TakeNumbers(2, 2, intMyBufferPos, arlBuffer.Count, false, false);
              if (lstDblTmp.Count > 1)
              {
                if (lstDblTmp[0] < lstDblTmp[1])
                  arlBuffer.Add("true");
                else
                  arlBuffer.Add("false");
              }
              else
                arlBuffer.Add(strEval);
            }
            else
              arlBuffer.Add(strEval);
            break;
          //----------------------------------------------------------------
          case ">":
            if (arlBuffer.Count - intMyBufferPos > 1)
            {
              List<double> lstDblTmp = TakeNumbers(2, 2, intMyBufferPos, arlBuffer.Count, false, false);
              if (lstDblTmp.Count > 1)
              {
                if (lstDblTmp[0] > lstDblTmp[1])
                  arlBuffer.Add("true");
                else
                  arlBuffer.Add("false");
              }
              else
                arlBuffer.Add(strEval);
            }
            else
              arlBuffer.Add(strEval);
            break;
          //----------------------------------------------------------------
          case "==":
            if (arlBuffer.Count - intMyBufferPos > 1)
            {
              List<double> lstDblTmp = TakeNumbers(2, 2, intMyBufferPos, arlBuffer.Count, false, false);
              if (lstDblTmp.Count > 1)
              {
                if (lstDblTmp[0] == lstDblTmp[1])
                  arlBuffer.Add("true");
                else
                  arlBuffer.Add("false");
              }
              else
                arlBuffer.Add(strEval);
            }
            else
              arlBuffer.Add(strEval);
            break;
          //----------------------------------------------------------------
          case "!=":
            if (arlBuffer.Count - intMyBufferPos > 1)
            {
              List<double> lstDblTmp = TakeNumbers(2, 2, intMyBufferPos, arlBuffer.Count, false, false);
              if (lstDblTmp.Count > 1)
              {
                if (lstDblTmp[0] != lstDblTmp[1])
                  arlBuffer.Add("true");
                else
                  arlBuffer.Add("false");
              }
              else
                arlBuffer.Add(strEval);
            }
            else
              arlBuffer.Add(strEval);
            break;
          //----------------------------------------------------------------
          case "<=":
            if (arlBuffer.Count - intMyBufferPos > 1)
            {
              List<double> lstDblTmp = TakeNumbers(2, 2, intMyBufferPos, arlBuffer.Count, false, false);
              if (lstDblTmp.Count > 1)
              {
                if (lstDblTmp[0] <= lstDblTmp[1])
                  arlBuffer.Add("true");
                else
                  arlBuffer.Add("false");
              }
              else
                arlBuffer.Add(strEval);
            }
            else
              arlBuffer.Add(strEval);
            break;
          //----------------------------------------------------------------
          case ">=":
            if (arlBuffer.Count - intMyBufferPos > 1)
            {
              List<double> lstDblTmp = TakeNumbers(2, 2, intMyBufferPos, arlBuffer.Count, false, false);
              if (lstDblTmp.Count > 1)
              {
                if (lstDblTmp[0] >= lstDblTmp[1])
                  arlBuffer.Add("true");
                else
                  arlBuffer.Add("false");
              }
              else
                arlBuffer.Add(strEval);
            }
            else
              arlBuffer.Add(strEval);
            break;
          //----------------------------------------------------------------
          case "count":
            arlBuffer.Add((double)(arlBuffer.Count - intMyBufferPos));
            break;
          //----------------------------------------------------------------
          case "countp":
            arlBuffer.Add((double)(intMyBufferPos - intParBufferPos));
            break;
          //----------------------------------------------------------------
          case "random":
            {
              List<double> lstDblTmp = TakeNumbers(2, 1, intMyBufferPos, arlBuffer.Count, false, false);
              if (lstDblTmp.Count == 2)
              {
                arlBuffer.Add((double)random.Next((int)lstDblTmp[0], (int)lstDblTmp[1]));
              }
              else if (lstDblTmp.Count == 1)
              {
                arlBuffer.Add((double)random.Next((int)lstDblTmp[0]));
              }
              else
              {
                arlBuffer.Add((double)random.Next());
              }
            }
            break;
          //----------------------------------------------------------------
          case "skip":
            if (arlBuffer.Count - intMyBufferPos > 0)
            {
              List<double> lstDblTmp = TakeNumbers(1, 1, intMyBufferPos, arlBuffer.Count, false, false);
              if (lstDblTmp.Count > 0)
              {
                intParSkip += (int)lstDblTmp[0];
              }
              else
                arlBuffer.Add(strEval);
            }
            else
              arlBuffer.Add(strEval);
            break;
          //----------------------------------------------------------------
          case "next":
            if (arlBuffer.Count - intMyBufferPos > 0)
            {
              List<double> lstDblTmp = TakeNumbers(1, 1, intMyBufferPos, arlBuffer.Count, false, false);
              if (lstDblTmp.Count > 0)
              {
                object objTmp = GetNextSymbol((int)lstDblTmp[0], intParPos, arlParSource);
                //arlBuffer.RemoveAt(arlBuffer.Count - 1);
                if (objTmp != null)
                {
                  LetterEval(objTmp, intMyBufferPos, intParBufferPos, intMyPos, arlMySource, intParPos, arlParSource, ref intMySkip, ref intParSkip, ref intVariablePop);
                  /*if (objTmp.GetType() == typeof(List<object>))
                    MittoEval((List<object>)objTmp, arlBuffer, intParPos, arlParentBuffer == null ? arlParSource : arlBuffer, ref intParSkip);
                  else if (objTmp.GetType() == typeof(double))
                    arlBuffer.Add(objTmp);
                  else if (((string)objTmp).Substring(0, 1) == "\"" || ((string)objTmp).Substring(0, 1) == "'")
                    arlBuffer.Add(objTmp);
                  else
                    SymbolEval((string)objTmp, arlBuffer, arlParentBuffer, intMyPos, arlMySource, intParPos, arlParSource, ref intParSkip, ref intMySkip);*/
                }
              }
              else
                arlBuffer.Add(strEval);
            }
            else
              arlBuffer.Add(strEval);
            break;
          //----------------------------------------------------------------
          case "take":
            if (arlBuffer.Count - intMyBufferPos > 0)
            {
              List<double> lstDblTmp = TakeNumbers(1, 1, intMyBufferPos, arlBuffer.Count, false, false);
              if (lstDblTmp.Count > 0)
              {
                arlBuffer.AddRange(TakeAny((int)lstDblTmp[0], 0, intParBufferPos, intMyBufferPos));
              }
              else
                arlBuffer.Add(strEval);
            }
            else
              arlBuffer.Add(strEval);
            break;
          //----------------------------------------------------------------
          case "taked":
            if (arlBuffer.Count - intMyBufferPos > 0)
            {
              List<double> lstDblTmp = TakeNumbers(1, 1, intMyBufferPos, arlBuffer.Count, false, false);
              if (lstDblTmp.Count > 0)
              {
                int intFound = 0;
                int intWhere = arlBuffer.Count;
                while (intFound != (int)lstDblTmp[0] && intFound < intMyBufferPos - intParBufferPos)
                {
                  arlBuffer.Insert(intWhere, arlBuffer[intMyBufferPos - intParBufferPos - 1 - intFound]);
                  intFound++;
                }
              }
              else
                arlBuffer.Add(strEval);
            }
            else
              arlBuffer.Add(strEval);
            break;
          //----------------------------------------------------------------
          case "eval":
            if (arlBuffer.Count - intMyBufferPos > 0)
            {
              List<string> lstTmp = TakeStrings(1, 1, intMyBufferPos, arlBuffer.Count, false);
              if (lstTmp.Count > 0)
              {
                Eval(lstTmp[0].Substring(1, lstTmp[0].Length - 2), false, false);
                //arlBuffer.RemoveAt(arlBuffer.Count - 1);
              }
              else
                arlBuffer.Add(strEval);
            }
            else
              arlBuffer.Add(strEval);
            break;
          //----------------------------------------------------------------
          case "print":
            if (arlBuffer.Count - intMyBufferPos > 0)
            {
              List<string> lstTmp = TakeStrings(-1, 1, intMyBufferPos, arlBuffer.Count, false);
              if (lstTmp.Count > 0)
              {
                StringBuilder strOutput = new StringBuilder();

                for (int i = 0; i < lstTmp.Count; i++)
                  strOutput.Append(lstTmp[i]);

                OutputF(strOutput.ToString(), 4);
                  //OutputF(lstTmp[i], 4);
              }
              else
                arlBuffer.Add(strEval);
            }
            else
              arlBuffer.Add(strEval);
            break;
          //----------------------------------------------------------------
          case "printline":
            if (arlBuffer.Count - intMyBufferPos > 0)
            {
              List<string> lstTmp = TakeStrings(-1, 1, intMyBufferPos, arlBuffer.Count, false);
              if (lstTmp.Count > 0)
              {
                StringBuilder strOutput = new StringBuilder();

                for (int i = 0; i < lstTmp.Count; i++)
                  strOutput.Append(lstTmp[i]);

                strOutput.Append('\n');
                OutputF(strOutput.ToString(), 4);
              }
              else
                arlBuffer.Add(strEval);
            }
            else
              arlBuffer.Add(strEval);
            break;
          //----------------------------------------------------------------
          case "+":
            if (arlBuffer.Count - intMyBufferPos > 1)
            {
              List<double> lstDblTmp = TakeNumbers(-1, 2, intMyBufferPos, arlBuffer.Count, false, false);
              if (lstDblTmp.Count > 1)
              {
                double dblTmp = 0;
                for (int i = 0; i < lstDblTmp.Count; i++)
                  dblTmp += lstDblTmp[i];

                arlBuffer.Add(dblTmp);
              }
              else
                arlBuffer.Add(strEval);
            }
            else
              arlBuffer.Add(strEval);
            break;
          //----------------------------------------------------------------
          case "-":
            if (arlBuffer.Count - intMyBufferPos > 0)
            {
              List<double> lstDblTmp = TakeNumbers(-1, 1, intMyBufferPos, arlBuffer.Count, false, false);
              if (lstDblTmp.Count > 0)
              {
                double dblTmp = lstDblTmp[0];
                if (lstDblTmp.Count > 1)
                {
                  for (int i = 1; i < lstDblTmp.Count; i++)
                    dblTmp -= lstDblTmp[i];
                }
                else
                  dblTmp = -dblTmp;

                arlBuffer.Add(dblTmp);
              }
              else
                arlBuffer.Add(strEval);
            }
            else
              arlBuffer.Add(strEval);
            break;
          //----------------------------------------------------------------
          case "*":
            if (arlBuffer.Count - intMyBufferPos > 1)
            {
              List<double> lstDblTmp = TakeNumbers(-1, 2, intMyBufferPos, arlBuffer.Count, false, false);
              if (lstDblTmp.Count > 1)
              {
                double dblTmp = 1;
                for (int i = 0; i < lstDblTmp.Count; i++)
                  dblTmp *= lstDblTmp[i];

                arlBuffer.Add(dblTmp);
              }
              else
                arlBuffer.Add(strEval);
            }
            else
              arlBuffer.Add(strEval);
            break;
          //----------------------------------------------------------------
          case "/":
            if (arlBuffer.Count - intMyBufferPos > 1)
            {
              List<double> lstDblTmp = TakeNumbers(-1, 2, intMyBufferPos, arlBuffer.Count, false, false);
              if (lstDblTmp.Count > 1)
              {
                double dblTmp = lstDblTmp[0];
                for (int i = 1; i < lstDblTmp.Count; i++)
                  dblTmp /= lstDblTmp[i];

                arlBuffer.Add(dblTmp);
              }
              else
                arlBuffer.Add(strEval);
            }
            else
              arlBuffer.Add(strEval);
            break;
          //----------------------------------------------------------------
          case "/n":
            if (arlBuffer.Count - intMyBufferPos > 1)
            {
              List<double> lstDblTmp = TakeNumbers(-1, 2, intMyBufferPos, arlBuffer.Count, false, true);
              if (lstDblTmp.Count > 1)
              {
                double dblTmp = lstDblTmp[0];
                for (int i = 1; i < lstDblTmp.Count; i++)
                  dblTmp /= lstDblTmp[i];

                arlBuffer.Add(dblTmp);
              }
              else
                arlBuffer.Add(strEval);
            }
            else
              arlBuffer.Add(strEval);
            break;
          //----------------------------------------------------------------
          default:
            arlBuffer.Add(strEval);
            break;
        }
      }
    }

    // Eval function (evals a parsed list)
    //--------------------------------------------------------------------------------------------------------------------------------
    public void MittoEval(List<object> arlEval, int intParBufferPos, int intParPos, List<object> arlParSource, ref int intParSkip)
    {
      int intMyBufferPos = arlBuffer.Count;
      int i;
      int intMySkip;
      int intVariablePop = 0;

      for (i = 0; i < arlEval.Count; i++)
      {
        intMySkip = 0;

        if (arlEval[i].GetType() == typeof(List<object>)) {
          MittoEval((List<object>)arlEval[i], intMyBufferPos, i, arlEval, ref intMySkip);
        }
        else if (arlEval[i].GetType() == typeof(double))
        {
          arlBuffer.Add(arlEval[i]);
        }
        else if (arlEval[i].GetType() == typeof(MittoClass))
        {
          arlBuffer.Add(arlEval[i]);
        }
        else if (IsString(arlEval[i]))
        {
          arlBuffer.Add(arlEval[i]);
        }
        else
        {
          SymbolEval((string)arlEval[i], intMyBufferPos, intParBufferPos, i, arlEval, intParPos, arlParSource, ref intMySkip, ref intParSkip, ref intVariablePop);
        }

        i += intMySkip;
      }

      intMySkip = 0;
      if (arlBuffer.Count - intMyBufferPos > 0)
      {
        BufferCheck(intMyBufferPos, intParBufferPos, i, arlEval, intParPos, arlParSource, ref intParSkip, ref intMySkip, ref intParSkip);
      }
    }

    // Main eval function (evals a string)
    //--------------------------------------------------------------------------------------------------------------------------------
    public void Eval(string txtEval)
    {
      Eval(txtEval, true, true);
    }

    public void Eval(string txtEval, bool blPrintOut, bool blClearBuffer)
    {
      txtEval = txtEval.Trim();
      if (txtEval.Length > 0)
      {
        List<object> arlSource = Parser(txtEval);

        if (blPrintParser)
          PrintSource(arlSource, 0);
        else
        {
          int intParSkip = 0;
          MittoEval(arlSource, 0, -1, arlSource, ref intParSkip);
          if (blPrintOut)
            PrintSource(arlBuffer, 0);
          if (blClearBuffer)
            arlBuffer.Clear();
        }
      }
    }

    // Parser
    //--------------------------------------------------------------------------------------------------------------------------------
    private List<object> Parser(string txtEval)
    {
      int intEnd;
      return Parser(txtEval, 0, out intEnd);
    }

    private List<object> Parser(string txtEval, int at, out int intEnd)
    {
      List<object> arlResult = new List<object>();

      int next;
      int atorig, atshift;
      int plen = txtEval.Length;
      int pos1; //, pos2;
      //int togo, togo2;
      string thiscmd;
      bool shouldAdd;
      double dblTmp;

      intEnd = -1;

      while (at <= plen)
      {
        next = FindNextWhite(txtEval, at);
        if (next == -1)
          next = plen;

        thiscmd = txtEval.Substring(at, next - at).Trim();

        atorig = at;
        atshift = 0;

        next++;

        if (thiscmd.Length > 0)
        {
          shouldAdd = true;

          if (thiscmd.Length > 1 && thiscmd.Substring(0, 1) == "#") //|| thiscmd.Substring(0, 1) == "$")
          {
            atshift = 1;
            at += atshift; // Remember that "at" changes, not "atorig", so atorig is the original

            shouldAdd = false;
          }

          // Check for full line comments // \n
          if (thiscmd.Length - atshift >= 2 && thiscmd.Substring(atshift, 2) == "//")
          {
            pos1 = txtEval.IndexOf("\n", at);
            if (pos1 == -1)
              next = plen;
            else
              next = pos1 + 1;

            //thiscmd = txtEval.Substring(atorig, next - atorig);

            shouldAdd = false;
          }
          // Check for block comments /* */
          else if (thiscmd.Length - atshift >= 2 && thiscmd.Substring(atshift, 2) == "/*")
          {
            pos1 = txtEval.IndexOf("*/", at);
            if (pos1 == -1)
              next = plen;
            else
              next = pos1 + 2;

            //thiscmd = txtEval.Substring(atorig, next - atorig);

            shouldAdd = false;
          }
          // Check for texts/strings " "
          else if (thiscmd.Length - atshift >= 1 && thiscmd.Substring(atshift, 1) == "\"")
          {
            pos1 = txtEval.IndexOf("\"", at + 1);
            if (pos1 == -1)
              next = plen;
            else
              next = pos1 + 1;

            thiscmd = Regex.Unescape(txtEval.Substring(atorig, next - atorig));
          }
          // Check for texts/strings ' '
          else if (thiscmd.Length - atshift >= 1 && thiscmd.Substring(atshift, 1) == "'")
          {
            pos1 = txtEval.IndexOf("'", at + 1);
            if (pos1 == -1)
              next = plen;
            else
              next = pos1 + 1;

            thiscmd = Regex.Unescape(txtEval.Substring(atorig, next - atorig));
          }
          // Check for envelopes ( )
          else if (thiscmd.Substring(atshift, 1) == "(")
          {
            arlResult.Add(Parser(txtEval, at + 1, out next));
            shouldAdd = false;
          }
          else if (thiscmd.IndexOf(')') != -1)
          {
            intEnd = atorig + thiscmd.IndexOf(')') + 1;
            thiscmd = txtEval.Substring(atorig, intEnd - atorig - 1);
            if (thiscmd.Length > 0)
            {
              if (double.TryParse(thiscmd, out dblTmp))
                arlResult.Add(dblTmp);
              else
                arlResult.Add(thiscmd);
            }
            return arlResult;
          }

          if (shouldAdd)
          {
            if (double.TryParse(thiscmd, out dblTmp))
              arlResult.Add(dblTmp);
            else
              arlResult.Add(thiscmd);
          }
        }

        at = next;
      }

      return arlResult;
    }





  }
}
