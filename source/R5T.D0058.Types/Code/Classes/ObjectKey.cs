using System;

using R5T.Stagira;


namespace R5T.D0058
{
    // BTW, see: https://docs.aws.amazon.com/AmazonS3/latest/userguide/object-keys.html
    public class ObjectKey : TypedString
    {
        #region Static

        public static ObjectKey From(string value)
        {
            var s3ObjectKey = new ObjectKey(value);
            return s3ObjectKey;
        }

        #endregion


        public ObjectKey(string value)
            : base(value)
        {
        }
    }
}
