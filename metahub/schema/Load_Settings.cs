package metahub.schema;

/**
 * @author Christopher W. Johnson
 */

class Load_Settings {
	public Namespace namespace;
	public bool auto_identity;
	
	public Load_Settings(Namespace namespace, bool auto_identity = false) {
		this.space = namespace;
		this.auto_identity = auto_identity;
	}
}