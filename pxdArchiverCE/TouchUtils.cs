using ParLibrary;
using Yarhl.FileSystem;

namespace pxdArchiverCE
{
    internal static class TouchUtils
    {
        internal static readonly DateTime RESET_TIME = new DateTime(1970, 1, 1);
        internal static readonly DateTime MAX_TIME = new DateTime(3000, 1, 1);


        /// <summary>
        /// Update the <see cref="Node"/>'s file date to the specified <see cref="DateTime"/>.
        /// </summary>
        /// <param name="node">The node to update.</param>
        /// <param name="time">The time to set.</param>
        internal static void SetTime(Node node, DateTime time)
        {
            if (time < RESET_TIME || time > MAX_TIME)
            {
                time = RESET_TIME;
            }

            if (node.IsContainer)
            {
                foreach (Node child in node.Children)
                {
                    SetTime(child, time);
                }
            }
            else
            {
                ParFile parFile = node.GetFormatAs<ParFile>();
                parFile.FileDate = time;
            }
        }


        /// <summary>
        /// Update the <see cref="Node"/>'s file date to epoch 0.
        /// </summary>
        /// <param name="node">The node to update.</param>
        internal static void ResetTime(Node node)
        {
            SetTime(node, RESET_TIME);
        }


        /// <summary>
        /// Update the <see cref="Node"/>'s file date to the current <see cref="DateTime"/>.
        /// </summary>
        /// <param name="node">The node to update.</param>
        internal static void SetCurrentTime(Node node)
        {
            DateTime currentTime = DateTime.Now;
            SetTime(node, currentTime);
        }
    }
}
