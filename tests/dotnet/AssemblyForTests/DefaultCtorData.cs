namespace AssemblyForTests
{
    public interface IData
    {
        string Name { get; }
    }

    public class DefaultCtorData : IData
    {
        public string Name { get; set; }

        public int[] Integers { get; set; }

        public OneCtorData OneCtorData { get; set; }

        public DefaultCtorData Clone()
        {
            return (DefaultCtorData)MemberwiseClone();
        }

        public double[,] Clone(double[,] matrix, out DefaultCtorData clone)
        {
            clone = Clone();
            return matrix;
        }

        #region Method with out arguments

        public bool TryGetValue(out double value)
        {
            value = 12.4;
            return true;
        }

        public bool TryGetObject(out DefaultCtorData data)
        {
            data = new DefaultCtorData { Name = "Out object" };
            return true;
        }

        public void UpdateValue(ref double value)
        {
            value += 1;
        }

        public void UpdateObject(ref DefaultCtorData data)
        {
            data = new DefaultCtorData { Name = "Ref object", Integers = data?.Integers };
        }

        #endregion
    }

    public class OneCtorData
    {
        public int Id { get; set; }

        public OneCtorData(int id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"{base.ToString()} #{Id}";
        }
    }

    public class ManyCtorData : IData
    {
        public int Id { get; }
        public string Name { get; }

        public ManyCtorData()
        {
            Id = -1;
            Name = "Default ctor #" + Id;
        }

        public ManyCtorData(int id)
        {
            Id = id;
            Name = "Integer ctor #" + id;
        }

        public ManyCtorData(string name)
        {
            Name = "String Ctor " + name;
        }

        public ManyCtorData(string name, int id)
        {
            Name = "String Ctor " + name;
        }

        public override string ToString()
        {
            return $"{base.ToString()} Name={Name}, Id={Id}";
        }
    }

    public class InheritedType : DefaultCtorData
    {
        public int Id { get; set; }
    }
}
