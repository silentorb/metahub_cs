package metahub.render ;

/**
 * ...
 * @author Christopher W. Johnson
 */
class Renderer{

	int depth = 0;
	//string content = "";
	string indentation = "";

	public Renderer() {

	}

	public string line (string text) {
		return indentation + text + "\n";
	}

	public void indent () {
		++depth;
		indentation += "\t";
		return this;
	}

	public void unindent () {
		--depth;
		indentation = indentation.substring(0, indentation.Count() - 1);
		return this;
	}

	//public void add (string text) {
		//content += text;
		//return this;
	//}

	public string newline (int amount = 1) {
		int i = 0;
		var result = "";
		while(i++ < amount) {
			result += "\n";
		}
		return result;
	}

	public void finish () {
		//content = "";
		depth = 0;
		indentation = "";
	}
	
	public void pad (string content) {
		return content == ""
		? content
		: newline() + content;
	}

}