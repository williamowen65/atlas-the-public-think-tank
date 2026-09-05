using Atlas.Graph;

//One caveat: don’t put business logic into Program.cs. It should merely construct objects, call their public operations, and show the results. Its job is to demonstrate the architecture—not become another version of Atlas.

//So yes: the console app is worthwhile for this particular rewrite, even though production Atlas will ultimately be driven by the web/API host.



var node = new Node("Climate adaptation");

Console.WriteLine($"Created node: {node.Title}");