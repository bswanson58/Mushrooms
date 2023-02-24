namespace Mushrooms.PatternTester {
    public record DisplayParameters {
        public  int     BulbCount { get; init; }
        public  double  RateMultiplier { get; init; }

        public static DisplayParameters DefaultDisplayParameters =>
        new DisplayParameters {
            BulbCount = 1,
            RateMultiplier = 1.0D
        };
    }
}
