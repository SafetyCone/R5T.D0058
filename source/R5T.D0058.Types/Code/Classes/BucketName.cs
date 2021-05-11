using System;

using R5T.Stagira;


namespace R5T.D0058
{
    // BTW, see: https://docs.aws.amazon.com/AmazonS3/latest/userguide/bucketnamingrules.html
    public class BucketName : TypedString
    {
        #region Static

        public static BucketName From(string value)
        {
            var s3BucketName = new BucketName(value);
            return s3BucketName;
        }

        #endregion


        public BucketName(string value)
            : base(value)
        {
        }
    }
}
