using metahub.imperative;
using metahub.imperative.schema;
using metahub.imperative.expressions;

namespace metahub.render
{
    public class Target
    {
        protected Renderer render = new Renderer();
        protected int line_count = 0;
        protected Overlord overlord;
        public Transmuter transmuter;

        public Target(Overlord overlord)
        {
            this.overlord = overlord;
            overlord.target = this;
        }

        public virtual void generate_dungeon_code(Dungeon dungeon)
        {

        }

        public virtual void generate_code2(Dungeon dungeon)
        {

        }

        public virtual void run(string output_folder)
        {

        }

        public string line(string text)
        {
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

        public string newline(int amount = 1)
        {
            ++line_count;
            return render.newline(amount);
        }

        public string pad(string content)
        {
            return content == ""
            ? content
            : newline() + content;
        }

        public virtual void analyze_expression(Expression expression)
        {
        }
    }
}