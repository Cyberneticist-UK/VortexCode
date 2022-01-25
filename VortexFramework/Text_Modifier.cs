

namespace Vortex
{
    public class Text_Modifier
    {
        /// <summary>
        /// This is really old - and really needs looking at in the future. It works, but My guess - really inefficient!
        /// </summary>
        /// <param name="rawhtml">The HTML to Parse</param>
        /// <param name="startone">The first character to replace</param>
        /// <param name="endone">The last character to replace</param>
        /// <param name="inputstring">What to replace this tag with</param>
        /// <returns></returns>
        public static string Compact_Tag(string rawhtml, string startone, string endone, string inputstring)
        {
            /**********************************************************
			 * Version: 1.0
			 * Description: 
			 *      This is one of my favourite algorithms ever! Basically it
			 *		Does a search and replace, but you only need to know the
			 *		start and end of the string you want to replace in the bigger string
			 *		and what you want to replace it with. For example, in HTML you may
			 *		want to replace all of the <FONT ... > Tags, not caring about what
			 *		differences there are in the tags between the FONT and the final >
			 ***********************************************************/
            // First of all, we need a counter which is how far we have searched the string so far:
            int counter = 0;

            // we need to know how long the input string is, for one of the while loops
            int counter2 = inputstring.Length;

            // initialise the variable for the inserting tag while loop
            int b = 0;

            // and we need to know how big the main array is
            int arraysize = 0;

            // Now we need to know how big the search strings are, so we make sure
            // we always start and end at the correct parts of the array
            int startonesize = startone.Length;
            int endonesize = endone.Length;

            // and we need an int which holds the position we are currently in 
            // within the string
            int position = 1;

            // and we need our lovely new string - actually a char array, as you need
            // that to be able to do a proper copy to procedure
            char[] newhtml = new char[5000000];

            // before entering the main program loop, find the first tag we are
            // searching for

            position = rawhtml.IndexOf(startone, counter);

            if (position == -1)
                return rawhtml;

            while (position > 0)
            {
                // copy everything upto that tag into the array
                rawhtml.CopyTo(counter, newhtml, arraysize, (position - counter));
                // move along to the correct index in the array
                arraysize = arraysize + (position - counter);

                // now we need to add the formatted string in, if present
                b = 0;
                while ((b != counter2) && (counter2 != 0))
                {
                    if (arraysize + b < 5000000)
                        newhtml[arraysize + b] = inputstring[b];
                    b++;
                }
                // again, move the index into the array along
                arraysize = arraysize + counter2;
                // now find the end of the tag and begin again!
                counter = rawhtml.IndexOf(endone, position + startonesize);
                position = rawhtml.IndexOf(startone, counter);
                counter = counter + endonesize;
            }
            // Need to copy whatever is left of the file once all tags removed
            rawhtml.CopyTo(counter, newhtml, arraysize, (rawhtml.Length - counter));
            // now we can see how big the array actually is, and only copy the
            // elements from the start to that value back into the string
            arraysize = arraysize + (rawhtml.Length - counter);
            // Finally, return the edited file.
            string returnstring = new string(newhtml);
            if (returnstring.Length - arraysize > 0)
                rawhtml = returnstring.Remove(arraysize, returnstring.Length - arraysize);
            return rawhtml;

        }
    }
}
