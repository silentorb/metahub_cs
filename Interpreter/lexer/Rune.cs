using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace runic.lexer
{
   public class Rune
   {
       public Whisper whisper;
       public string text;

       public Rune(Whisper whisper, string text)
       {
           this.whisper = whisper;
           this.text = text;
       }

       public int length
       {
           get { return text != null ? text.Length : 0; }
       }

   }
}
