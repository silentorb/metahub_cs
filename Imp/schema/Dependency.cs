

namespace imperative.schema
{
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Dependency
{
	public Dungeon dungeon;
	public bool allow_partial = true;

    public Dependency(Dungeon dungeon)
	{
		this.dungeon = dungeon;
	}
	
}
}