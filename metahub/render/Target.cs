using metahub.imperative;
using metahub.imperative.schema;
using metahub.logic.schema;

namespace metahub.render
{

public class Target{
	protected Railway railway;
	protected Renderer render = new Renderer();
	protected int line_count = 0;
	protected Imp imp;
	
	public Target(Railway railway, Imp imp) {
		this.railway = railway;
		this.imp = imp;
	}

	public void generate_rail_code (Dungeon dungeon) {
	
	}
	
	public virtual void run (string output_folder) {

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