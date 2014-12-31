using System;
using System.Collections.Generic;
using System.Linq;
using metahub.imperative.code;
using metahub.imperative.schema;
using metahub.imperative.types;
using metahub.logic;
using metahub.logic.schema;
using metahub.render;
using metahub.schema;
using Constraint = metahub.logic.schema.Constraint;
using Scope = metahub.logic.Scope;
using Node_Type = metahub.logic.types.Node_Type;
using Node = metahub.logic.types.Node;

namespace metahub.imperative
{

    public class Imp
    {
        public Railway railway;
        public List<Dungeon> dungeons = new List<Dungeon>();
        Dictionary<Rail, Dungeon> rail_map = new Dictionary<Rail, Dungeon>();

        public Imp(Hub hub, string target_name)
        {
            railway = new Railway(hub, target_name);
        }

        public void run(Logician logician, Target target)
        {
            //process(root, null);
            generate_code(target);

            foreach (var constraint in logician.constraints)
            {
                implement_constraint(constraint);
            }

            flatten();

            post_analyze();
        }

        public void generate_code(Target target)
        {

            foreach (var region in railway.regions.Values)
            {
                if (region.is_external)
                    continue;

                foreach (var rail in region.rails.Values)
                {
                    if (rail.is_external)
                        continue;

                    Dungeon dungeon = new Dungeon(rail, this);
                    dungeons.Add(dungeon);
                    rail_map[rail] = dungeon;
                }
            }

            finalize();

            foreach (var dungeon in dungeons)
            {
                dungeon.generate_code1();
                target.generate_rail_code(dungeon);
                dungeon.generate_code2();
            }
        }

        void finalize()
        {
            foreach (var dungeon in dungeons)
            {
                dungeon.rail.finalize();
            }
        }

        void post_analyze()
        {
            foreach (var dungeon in dungeons)
            {
                dungeon.post_analyze_many(dungeon.code);
            }
        }

        public void flatten()
        {
            foreach (var dungeon in dungeons)
            {
                dungeon.flatten();
            }
        }

        public Dungeon get_dungeon(Rail rail)
        {
            return rail_map[rail];
        }

        //Node create_lambda_constraint (metahub Node.meta.types.Constraint, Scope scope) {
        //throw "";
        //var rail = get_rail(scope.trellis);
        //metahub.logic.schema.Constraint constraint = new metahub.logic.schema.Constraint(Node, this);
        //var tie = Parse.get_end_tie(constraint.reference);
        //trace("tie", tie.rail.name + "." + tie.name);
        //tie.constraints.Add(constraint);
        //constraints.Add(constraint);
        //return null;
        //}

        public Rail get_rail(Trellis trellis)
        {
            return railway.get_rail(trellis);
        }

        public void implement_constraint(Constraint constraint)
        {
            //var ties = Parse.get_endpoints(constraint.first);
            foreach (var tie in constraint.endpoints.Where(tie => tie != null))
            {
                if (tie.type == Kind.list)
                {
                    List_Code.generate_constraint(constraint, this);
                }
                else
                {
                    var dungeon = get_dungeon(tie.rail);
                    dungeon.concat_block(tie.tie_name + "_set_pre", Reference.constraint(constraint, tie, this));
                }
            }
        }

        public Expression translate(Node expression, Scope scope = null)
        {
            switch (expression.type)
            {
                case Node_Type.literal:
                    return new Literal(((metahub.logic.types.Literal_Value)expression).value, new Signature(Kind.unknown));

                case Node_Type.function_call:
                    //metahub.logic.types.Function_Call func = expression;
                    //return new Function_Call(func.name, [translate(func.input)]);
                    throw new Exception("Not implemented.");

                case Node_Type.path:
                    return convert_path(((metahub.logic.types.Reference_Path)expression).children, scope);

                case Node_Type.array:
                    return new Create_Array(translate_many(((metahub.logic.types.Array_Expression)expression).children, scope));

                case Node_Type.block:
                    return new Create_Array(translate_many(((metahub.logic.types.Block)expression).children, scope));

                case Node_Type.lambda:

                    return null;
                //metahub.logic.types.Lambda lambda = Node;
                //return new Anonymous_(lambda.parameters.map(function(p)=> new Parameter(p.name, p.signature)),
                //lambda.expressions.map((e)=> translate(e, lambda.scope))
                //);
                //
                //case metahub.logic.types.Expression_Type.constraint:
                //return create_lambda_constraint(Node, scope);

                case Node_Type.property:
                    return convert_path(new List<Node> {expression});

                default:
                    throw new Exception("Cannot convert node " + expression.type + ".");
            }
        }

        public IEnumerable<Expression> translate_many(IEnumerable<Node> nodes, Scope scope)
        {
            return nodes.Select(n => translate(n, scope));
        }

        public Expression convert_path(Node[] path, Scope scope)
        {
            if (path.Length == 0)
                throw new Exception("Cannot convert empty path.");

            if (path.Length == 1)
                return translate(path[0], scope);

            return package_path(translate_many(path, scope));
        }

        Expression package_path(IEnumerable<Expression> path)
        {
            if (path.Count() == 1)
                return path.First();

            return new Path(path);  
        }

        public Expression convert_path(List<metahub.logic.types.Node> path, Scope scope = null)
        {
            List<Expression> result = new List<Expression>();
            Rail rail = null;

            if (path[0].type == Node_Type.property)
            {
                rail = ((metahub.logic.types.Property_Reference)path[0]).tie.get_abstract_rail();
            }
            //else
            //{
            //    rail = ((metahub.logic.types.Array_Expression)path[0])
            //}
            foreach (var token in path)
            {
                switch (token.type)
                {
                    case Node_Type.property:
                        var property_token = (metahub.logic.types.Property_Reference)token;
                        var tie = rail.all_ties[property_token.tie.name];
                        if (tie == null)
                            throw new Exception("tie is null: " + property_token.tie.fullname());

                        result.Add(new Property_Expression(tie));
                        rail = tie.other_rail;
                        break;

                    case Node_Type.array:
                        //result.Add(translate(token, scope));
                        break;

                    case Node_Type.variable:
                        var variable = (metahub.logic.types.Variable) token;
                        result.Add(new Variable(variable.name));
                        rail = variable.signature.rail;
                        break;

                    case Node_Type.function_call:
                    case Node_Type.function_scope:
                        var function_token = (metahub.logic.types.Function_Call)token;
                        result.Add(new Function_Call(function_token.name, new List<Expression>(), true));
                        break;

                    default:
                        throw new Exception("Invalid path token: " + token.type);
                }
            }
            return package_path(result);
        }

        public static Node[] simplify_path(Node[] path)
        {
            return path.Where(t => t.type == Node_Type.property).ToArray();
        }
    }
}