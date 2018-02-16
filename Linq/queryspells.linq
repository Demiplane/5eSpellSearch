<Query Kind="Program">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
</Query>

void Main()
{
	var jsonSpellDatabasePath = Util.ReadLine();
	
	var spells = JsonConvert.DeserializeObject<IEnumerable<Spell>>(File.ReadAllText(jsonSpellDatabasePath));
		
		spells
	
		.Where(f => f.Level() <= 5)
		.Where(f => !f.Tags.Contains("bard"))
		.Where(f => !f.ConsumesComponents())
		
	
		.Dump();
		
		spells.Where(f => f.Tags.Contains("bard")).Dump();
}

public static class SpellExtensions
{
	public static bool Concentration(this Spell spell) => spell.Duration.IndexOf("concentration", StringComparison.OrdinalIgnoreCase) >= 0;
	public static bool Ritual(this Spell spell) => spell.Type.IndexOf("ritual", StringComparison.OrdinalIgnoreCase) >= 0;
	public static int Level(this Spell spell) => spell.Type.IndexOf("cantrip", StringComparison.OrdinalIgnoreCase) >= 0 ? 0 : spell.Type == string.Empty ? -1 : Convert.ToInt32(spell.Type[0].ToString());
	public static string School(this Spell spell) => spell.Type.EndsWith("cantrip") ? spell.Type.Split(' ').First().ToLower() : spell.Type.Split(' ').Last();
	public static bool CastAsAction(this Spell spell) => spell.CastingTime.Contains("action") && !spell.CastAsBonusAction();
	public static bool CastAsBonusAction(this Spell spell) => spell.CastingTime.Contains("bonus");
	public static bool CastAsReaction(this Spell spell) => spell.CastingTime.Contains("reaction");
	public static bool CanCastWithinOneTurn(this Spell spell) => spell.CastAsAction() || spell.CastAsBonusAction() || spell.CastAsReaction();
	public static bool VerbalComponents(this Spell spell) => spell.Components.Split('(').First().Contains('V');
	public static bool SomaticComponents(this Spell spell) => spell.Components.Split('(').First().Contains('S');
	public static bool MaterialComponents(this Spell spell) => spell.Components.Split('(').First().Contains('M');
	public static bool CostlyComponents(this Spell spell) => spell.Components.Contains("gp") || spell.Components.Contains("worth") || spell.Components.Contains("sp");
	public static bool ConsumesComponents(this Spell spell) => spell.Components.Contains("consume");
	public static bool CanUpCast(this Spell spell) => spell.AtHigherLevels != string.Empty;
}

public class Spell
{
	public string Name { get; set; }
	public string Type { get; set; }
	public string CastingTime { get; set; }
	public string Range { get; set; }
	public string Components { get; set; }
	public string Duration { get; set; }
	public string[] Tags { get; set; }
	public string Description { get; set; }
	public string AtHigherLevels { get; set; }
	public Uri GrimoireLink { get; set; }
	public string Book { get; set; }
	public string Page { get; set; }
}