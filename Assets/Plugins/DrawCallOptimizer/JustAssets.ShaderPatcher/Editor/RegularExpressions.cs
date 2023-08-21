namespace JustAssets.ShaderPatcher
{
    public class RegularExpressions
    {
        public static string ClosingBracket = "}";

        public static string Property = @"(?<Indent>\s*)(?<Attributes>\[[^]]+\] ?)?(?<Name>[_A-Za-z0-9]+) ?\( ?""(?<Description>[^""]*)"" ?, ?(?<Type>[A-Za-z23]+(?: ?\([^)]+\))?) ?\) ?= ?(?<DefaultValue>[0-9.A-Za-z, ()\-""]+(?: ?{})?)";
    }
}