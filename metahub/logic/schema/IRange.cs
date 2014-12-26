using System.Collections.Generic;

namespace metahub.logic.schema
{

/**
 * @author Christopher W. Johnson
 */

public interface IRange 
{
    int type { get; set; }
	List<Tie> path { get; set; }
}
}