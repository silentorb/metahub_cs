using System;
using System.Collections.Generic;
using System.Linq;
using metahub.imperative.schema;
using metahub.imperative.summoner;



namespace metahub.imperative.expressions
{

    public delegate Expression Expression_Generator(Summoner.Context context);

    public class Expression
    {
        public Expression_Type type;
        public Expression child
        {
            get { return children.Count > 0 ? children[0] : null; }
            set
            {
                if (children.Count > 0 && children[0] == value)
                    return;

                if (children.Count > 0)
                    children[0].parent = null;

                children.Clear();
                if (value != null)
                {
                    value.parent = this;
                }

            }
        }

        public virtual bool is_token
        {
            get { return false; }
        }

        /*
                 private Expression _child;
        public Expression child
        {
            get { return _child; }
            set
            {
                _child = value;
                if (_child != null)
                {
                    _child.parent = this;
                }
            }
        }*/
        public string stack_trace;

        private Expression _parent = null;
        public Expression parent
        {
            get { return _parent; }
            set
            {
                if (value != null && !value.children.Contains(this))
                    value.children.Add(this);

                if (value == _parent)
                    return;

                if (_parent != null && _parent.children.Contains(this))
                    _parent.children.Remove(this);

                _parent = value;

            }
        }
        public List<Expression> children = new List<Expression>();

        protected Expression(Expression_Type type, Expression child = null)
        {
            stack_trace = Environment.StackTrace;
            this.type = type;
            this.child = child;
            if (child != null)
                child.parent = this;
        }

        public void add(Expression expression)
        {
            expression.parent = this;
        }

        public void add(IEnumerable<Expression> expressions)
        {
            foreach (var child in expressions)
            {
                child.parent = this;
            }
        }

        public virtual Profession get_profession()
        {
            throw new Exception("Not implemented.");
        }

        public Expression get_end()
        {
            var result = this;
            while (result.child != null && (result.child.type == Expression_Type.property || result.child.type == Expression_Type.portal))
            {
                result = result.child;
            }

            return result;
        }

        public List<Expression> get_chain()
        {
            var result = new List<Expression>();
            var current = this;
            while (current != null && (current.type == Expression_Type.property || current.type == Expression_Type.portal))
            {
                result.Add(current);
                current = current.child;
            }

            return result;
        }

        public virtual Expression clone()
        {
            throw new Exception("Not implemented.");
        }

        //public void disconnect_parent()
        //{
        //    if (parent == null)
        //        throw new Exception("Cannot disconnect parent.");

        //    if (parent.child != this)
        //        throw new Exception("parent child mixup.");

        //    parent.child = null;
        //    parent = null;
        //}

        public IEnumerable<Expression> aggregate()
        {
            return new[] { this }.Concat(children.SelectMany(c => c.aggregate()));

            //var result = new List<Expression>();
            //result.Add(this);
            //foreach (var child in children)
            //{
            //    result.AddRange(child.aggregate());
            //}

            //return result;
        }

        public virtual bool is_empty()
        {
            return false;
        }
    }
}