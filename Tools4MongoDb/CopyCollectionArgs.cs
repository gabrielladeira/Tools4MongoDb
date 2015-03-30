using PowerArgs;

namespace Tools4MongoDb
{
    public class CopyCollectionArgs
    {
        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("MongoDb Source Url")]
        public string SourceUrl { get; set; }

        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("MongoDb Source Database")]
        public string SourceDatabase { get; set; }

        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("MongoDb Source Collection")]
        public string SourceCollection { get; set; }


        [ArgDescription("MongoDb Destiny Url")]
        public string DestinyUrl { get; set; }

        [ArgDescription("MongoDb Destiny Database")]
        public string DestinyDatabase { get; set; }

        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("MongoDb Destiny Collection")]
        public string DestinyCollection { get; set; }


        [ArgDefaultValue(false)]
        [ArgDescription("Clear Destiny Collection before starts")]
        public bool ClearDestiny { get; set; }


        [ArgDescription("Max Documents will be imported")]
        public int? MaxDocuments { get; set; }


        [ArgDescription("Sampling Documents to especifed max documents")]
        public long? SamplingMaxDocuments { get; set; }

        [ArgRange(0, 1)]
        [ArgDescription("Sampling Documents to percentual of total")]
        public double? SamplingPercentualDocuments { get; set; }


        [ArgDefaultValue(DuplicateKey.Skip)]
        [ArgDescription("Handle when a duplicate key exception was throw")]
        public DuplicateKey OnDuplicateKey { get; set; }
    }
}