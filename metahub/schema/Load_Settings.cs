namespace metahub.schema
{



/**
 * @author Christopher W. Johnson
 */
public class Load_Settings {
	public Namespace space;
	public bool auto_identity;
	
	public Load_Settings(Namespace space, bool auto_identity = false) {
		this.space = space;
		this.auto_identity = auto_identity;
	}
}
}
