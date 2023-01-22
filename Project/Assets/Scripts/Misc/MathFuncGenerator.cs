using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

// https://stackoverflow.com/questions/32726576/generate-random-mathematical-functions
// https://mathparser.org/mxparser-math-collection/unary-functions/
// https://mathparser.org/mxparser-math-collection/binary-functions/
// https://mathparser.org/mxparser-math-collection/constants/

public class MathFuncGenerator
{
    private Node tree;
    private Dictionary<Type, List<IFunc>> functions = new Dictionary<Type, List<IFunc>>();

    public void AddConstantFunc( Func<float> exec, Func<string> toStr ) 
    {
        AddCustomFunc( new ConstantFunc() { generator = x => exec(), toString = x => toStr() } );
    }

    public void AddUnaryFunc( Func<float, float> exec, Func<string, string> toStr ) 
    {
        AddCustomFunc( new UnaryFunc() { generator = x => exec( x[0] ), toString = x => toStr( x[0] ) } );
    }

    public void AddBinaryFunc( Func<float, float, float> exec, Func<string, string, string> toStr ) 
    {
        AddCustomFunc( new BinaryFunc() { generator = x => exec( x[0], x[1] ), toString = x => toStr( x[0], x[1] ) } );
    }

    public void AddCustomFunc<T>( T func ) where T : IFunc
    {
        if( functions.TryGetValue( func.GetType(), out var list ) )
            list.Add( func );
        else
            functions.Add( func.GetType(), new List<IFunc>() { func } );
    }

    public MathFuncGenerator DefaultValues( float constantMin, float constantMax )
    {
        AddConstantFunc( () => MathF.PI, () => "PI" );
        AddConstantFunc( () => MathF.E, () => "E" );
        AddCustomFunc( new RandomConstantFunc(){ constantMin = constantMin, constantMax = constantMax } );

        AddUnaryFunc( x => -x, x => $"-{x}" );
        AddUnaryFunc( x => MathF.Sin( x ), x => $"Sin({x})" );
        AddUnaryFunc( x => MathF.Cos( x ), x => $"Cos({x})" );
        AddUnaryFunc( x => MathF.Tan( x ), x => $"Tan({x})" );
        AddUnaryFunc( x => MathF.Acos( Utility.Mod( x, 2.0f ) - 1.0f ), x => $"ACos({x})" );
        AddUnaryFunc( x => MathF.Asin( Utility.Mod( x, 2.0f ) - 1.0f ), x => $"ASin({x})" );
        AddUnaryFunc( x => MathF.Atan( x ), x => $"ATan({x})" );
        AddUnaryFunc( x => x > 0.0f ? MathF.Log( x ) : 0.0f, x => $"Log({x})" );
        AddUnaryFunc( x => x > 0.0f ? MathF.Log10( x ) : 0.0f, x => $"Log{Utility.ToSubscript("10")}({x})" );
        AddUnaryFunc( x => x > 0.0f ? MathF.Sqrt( x ) : 0.0f, x => $"Sqrt({x})" );
        AddUnaryFunc( x => MathF.Sign( x ), x => $"Sign({x})" );
        AddUnaryFunc( x => MathF.Round( x ), x => $"Round({x})" );
        AddUnaryFunc( x => MathF.Abs( x ), x => $"Abs({x})" );
        AddUnaryFunc( x => x > 0.0f ? MathF.Cbrt( x ) : 0.0f, x => $"Cbrt({x})" );
        AddUnaryFunc( x => MathF.Ceiling( x ), x => $"Ceil({x})" );
        AddUnaryFunc( x => MathF.Floor( x ), x => $"Floor({x})" );

        AddBinaryFunc( ( a, b ) => a + b, ( a, b ) => $"{a} + {b}" );
        AddBinaryFunc( ( a, b ) => a - b, ( a, b ) => $"{a} - {b}" );
        AddBinaryFunc( ( a, b ) => a * b, ( a, b ) => $"{a} * {b}" );
        AddBinaryFunc( ( a, b ) => a.SafeDivide( b ), ( a, b ) => $"{a} / {b}" );
        AddBinaryFunc( ( a, b ) => MathF.Max( a, b ), ( a, b ) => $"Max({a}, {b})" );
        AddBinaryFunc( ( a, b ) => MathF.Min( a, b ), ( a, b ) => $"Min({a}, {b})" );
        AddBinaryFunc( ( a, b ) => ( a > 0.0f || a == Mathf.Floor( a + float.Epsilon ) ) ? MathF.Pow( a, b ) : 0.0f, ( a, b ) =>
        {
            if( float.TryParse( b, out float _ ) )
                return $"{a}^{Utility.ToSuperscript( b )}";
            return $"{a}^({b})";
        } );

        return this;
    }

    public void Reset()
    {
        functions = new Dictionary<Type, List<IFunc>>();
        tree = null;
    }

    public void ConstructTree( int maxDepth )
    {
        tree = new Node();
        tree.GenerateTree( this, 0, maxDepth );
    }

    public float EvaluateTree()
    {
        Debug.Assert( tree != null, "Cannot evaluate tree before one has been constructed" );
        return tree != null ? tree.EvaluateTree() : 0.0f;
    }

    public string TreeToString()
    {
        Debug.Assert( tree != null, "Cannot get tree as string before one has been constructed" );
        return tree != null ? tree.TreeToString() : String.Empty;
    }

    public abstract class IFunc 
    {
        public abstract int NumArgs();
        public Func<float[], float> generator;
        public Func<string[], string> toString;
    }

    class ConstantFunc : IFunc { public override int NumArgs() { return 0; } }
    class UnaryFunc : IFunc { public override int NumArgs() { return 1; } }
    class BinaryFunc : IFunc { public override int NumArgs() { return 2; } }

    [Serializable]
    public class RandomConstantFunc : IFunc, ISerializable
    {
        public RandomConstantFunc() {}
        public override int NumArgs() { return 0; }

        protected RandomConstantFunc( SerializationInfo info, StreamingContext context )
        {
            constantMin = info.GetSingle( "min" );
            constantMax = info.GetSingle( "max" );
            float rand = UnityEngine.Random.Range( MathF.Min( constantMin, constantMax ), MathF.Max( constantMin, constantMax ) );
            generator = x => rand;
            toString = x => rand.ToString();
        }

        void ISerializable.GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.AddValue( "min", constantMin );
            info.AddValue( "max", constantMax );
        }

        public float constantMin, constantMax;
    }

    class Node
    {
        public Node GenerateTree( MathFuncGenerator data, int depth, int maxDepth )
        {
            var( randomType, functions ) = data.functions.RandomItem();
            func = functions.RandomItem();

            if( func.NumArgs() > 0 )
            {
                children = new List<Node>();
                children.Resize( func.NumArgs(), () => new Node().GenerateTree( data, depth + 1, maxDepth ) );
            }

            // If set as serialisable, construct an instance of it by deep copying (serialise/deserialise)
            // This is needed for entries like RandomConstantFunc that needs to regenerate for each individual node usage (with unique data)
            if( func.GetType().IsSerializable )
            {
                var newFunc = func.DeepCopy();
                func = newFunc;
            }

            return this;
        }

        public float EvaluateTree()
        {
            var args = children?.Select( x => x.EvaluateTree() );
            var value = func.generator( args?.ToArray() );
            if( float.IsNaN( value ) )
            {
                Debug.LogError( $"EvaluateTree returned a NaN value (Node: {TreeToString()})" );
                return 0.0f;
            }
            return value;
        }

        public string TreeToString()
        {
            var args = children?.Select( x => x.TreeToString() );
            return func.toString( args?.ToArray() );
        }

        public List<Node> children;
        public IFunc func;
    }
}