using metahub.imperative;
using metahub.imperative.schema;
using metahub.logic.schema;

namespace metahub.render
{

public class Target{
	Railway railway;
	Renderer render = new Renderer();
	int line_count = 0;
	Imp imp;
	
	public Target(Railway railway, Imp imp) {
		this.railway = railway;
		this.imp = imp;
	}

	public void generate_rail_code (Dungeon dungeon) {
	
	}
	
	public void run (string output_folder) {

	}
	
	public string line (string text) {
		++line_count;
		return render.line(text);
	}

    public Renderer indent()
    {
		return render.indent();
	}

    public Renderer unindent()
    {
		return render.unindent();
	}

	public string newline (int amount = 1) {
		++line_count;
		return render.newline(amount);
	}

    public string pad(string content)
    {
		return content == ""
		? content
		: newline() + content;
	}

}
}