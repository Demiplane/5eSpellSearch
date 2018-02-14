<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <NuGetReference>HtmlAgilityPack</NuGetReference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>HtmlAgilityPack</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
</Query>

void Main()
{
	var jsonSpellDatabasePath = @"";
	
	var spellUris = AcquireSpellUrisFromGrimoire();

	var downloadedIndividualSpells = spellUris
		.Select(DownloadSpellAsJson);

	Save(downloadedIndividualSpells, jsonSpellDatabasePath);
}

public void Save(IEnumerable<Spell> spells, string path)
{
	File.WriteAllText(path, JsonConvert.SerializeObject(spells));
}

public Spell DownloadSpellAsJson(Uri uri)
{
	var document = new HtmlWeb()
		.Load(uri)
		.DocumentNode;

	var Clean = new Func<string, string>(s => s.Replace(":", "").Trim());

	var leftData = document.SelectNodes(@"html/body/div").Last().SelectNodes("div").First().SelectNodes("p");
	var header = document.Descendants("header").Where(d => d.HasClass("post-header")).First().Descendants("h2").First();

	var name = header.FirstChild.InnerText;
	var spellType = header.LastChild.InnerText;
	var castingTime = leftData.First().LastChild.OuterHtml;
	var range = leftData.Skip(1).First().LastChild.OuterHtml;
	var components = leftData.Skip(2).First().LastChild.OuterHtml;
	var duration = leftData.Skip(3).First().LastChild.OuterHtml;
	var tags = document.SelectNodes(@"html/body/div").Last().SelectNodes("div").First().Descendants("a").Select(f => f.InnerText).ToArray();

	var skip = document.Descendants("article").First().SelectNodes("p").Any(f => f.InnerText.StartsWith("Components:")) ? 5 : 0;

	var articlePNodes = document.Descendants("article").First().SelectNodes("p");

	if (skip > 0)
	{
		spellType = articlePNodes.First().FirstChild.InnerText;
		castingTime = articlePNodes.Skip(1).First().LastChild.InnerText;
		range = articlePNodes.Skip(2).First().LastChild.InnerText;
		components = articlePNodes.Skip(3).First().LastChild.InnerText;
		duration = articlePNodes.Skip(4).First().LastChild.InnerText;
	}

	var description = string.Join(Environment.NewLine, articlePNodes.Skip(skip).Reverse().Skip(1).Reverse().Select(p => p.InnerHtml));

	var higherLevelsSplitToken = "<strong>At Higher Levels.</strong>";

	var atHigherLevels = description.Contains(higherLevelsSplitToken) ? description.Split(new[] { higherLevelsSplitToken }, StringSplitOptions.RemoveEmptyEntries).Last().Trim() : string.Empty;
	description = description.Split(new[] { higherLevelsSplitToken }, StringSplitOptions.RemoveEmptyEntries).First().Trim();

	var extraInformationLine = document.Descendants("article").First().SelectNodes("p").Last().InnerText.Trim();

	var book = extraInformationLine.Split(' ').Last().Split('.').First();
	var page = extraInformationLine.Split(' ').Last().Split('.').Last();

	return new Spell()
	{
		Name = name.Trim(),
		Type = spellType.Trim(),
		CastingTime = Clean(castingTime),
		Range = Clean(range),
		Components = Clean(components),
		Duration = Clean(duration),
		AtHigherLevels = atHigherLevels.Trim(),
		Tags = tags,
		Description = description.Trim(),
		GrimoireLink = uri,
		Book = book.Trim(),
		Page = page.Trim(),
	};
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

public IEnumerable<Uri> AcquireSpellUrisFromGrimoire()
{
	return new HtmlWeb()
		.Load(@"https://thegrimoire.xyz/")
		.DocumentNode
		.Descendants("li")
		.Where(li => li.HasClass("spell-item"))
		.Select(li => li.Descendants("a").First().Attributes.First(a => a.Name == "href").Value)
		.Where(f => !f.Contains("Elemental-Evil-"))
		.Select(uri => new Uri(new Uri(@"https://thegrimoire.xyz"), uri))
		;
}