using System;

namespace Vortex
{
    [Serializable]
    public class NodeLink
    {
        #region Public Fields

        /// <summary>
        /// The Parent Item ID.
        /// </summary>
        public Guid Item1ID = System.Guid.NewGuid();
        /// <summary>
        /// The Child Item ID
        /// </summary>
        public Guid Item2ID = System.Guid.NewGuid();
        /// <summary>
        /// The Type for this link. The Standard is "Standard".
        /// </summary>
        public Guid LinkTypeID = System.Guid.NewGuid();
        /// <summary>
        /// Any extra information needed for the link
        /// </summary>
        public string ExtraInfo;
        /// <summary>
        /// Can be used to confirm true or false
        /// </summary>
        public bool Confirmed;
        /// <summary>
        /// Can store numerical data
        /// </summary>
        public NodeValue ValueInfo;
        /// <summary>
        /// Used to join children together via a path
        /// </summary>
        public Guid PathID;
        /// <summary>
        /// When the link was created
        /// </summary>
        public DateTime LinkCreated = System.DateTime.Now;
        /// <summary>
        /// When the link was last updated
        /// </summary>
        public DateTime LinkLastUpdated = System.DateTime.Now;
        #endregion
        public object GetValue(Field F, string Format = null)
        {
            if (Format != null)
                Format = Format.Replace("_", " ");
            switch (F)
            {
                case Field.Item1ID:
                    return Item1ID;
                case Field.Item2ID:
                    return Item2ID;
                case Field.LinkTypeID:
                    return LinkTypeID;
                case Field.ExtraInfo:
                    return ExtraInfo;
                case Field.Confirmed:
                    return Confirmed;
                case Field.ValueInfo:
                    return (Decimal)ValueInfo;
                case Field.PathID:
                    return PathID;
                case Field.LinkCreated:
                    if (Format != null)
                        return LinkCreated.ToString(Format);
                    return LinkCreated;
                case Field.LinkLastUpdated:
                    if (Format != null)
                        return LinkLastUpdated.ToString(Format);
                    return LinkLastUpdated;
                default:
                    return null;
            }
        }
    }
}
