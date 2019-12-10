namespace AssemblyForTests
{
    public class DefaultCtorData
    {
        public string Name { get; set; }

        public int[] Integers { get; set; }

        public OneCtorData OneCtorData { get; set; }

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
        private readonly int _id;

        public OneCtorData(int id)
        {
            _id = id;
        }

        public override string ToString()
        {
            return $"{base.ToString()} #{_id}";
        }
    }

    public class ManyCtorData
    {
        private readonly int _id;
        private readonly string _name;

        public ManyCtorData()
        {
            _id = -1;
            _name = "Default ctor #" + _id;
        }

        public ManyCtorData(int id)
        {
            _id = id;
            _name = "Integer ctor #" + id;
        }

        public ManyCtorData(string name)
        {
            _name = "String Ctor " + name;
        }

        public override string ToString()
        {
            return $"{base.ToString()} Name={_name}, Id={_id}";
        }
    }
}
