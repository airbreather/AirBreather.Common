using System.Text;

namespace AirBreather.Core.Utilities
{
    // Since .ToString() is intended mainly for debugging purposes,
    // this is just going to be something simple without worrying too much about formatting.
    public interface IStringCreator
    {
        IStringCreator AddProperty<T>(string propertyName, T propertyValue);
        string End();
    }

    public static class ToStringUtility
    {
        // generic-with-exemplar is just so that I can write:
        //     ToStringUtility.Begin(this)
        // instead of one of these:
        //     ToStringUtility.Begin<MyType>()
        //     ToStringUtility.Begin(typeof(MyType))
        //     ToStringUtility.Begin("MyType")
        // notice the lack of MyType in the generic-with-exemplar version.
        //
        // example usage (inside the Person.ToString() override):
        //     return ToStringUtility.Begin(this)
        //                           .AddProperty("FirstName", this.FirstName)
        //                           .AddProperty("MiddleName", this.MiddleName)
        //                           .AddProperty("LastName", this.LastName)
        //                           .End();
        // example output:
        //     Person[FirstName=John, MiddleName=(null), LastName=Smith]
        public static IStringCreator Begin<T>(T exemplar)
        {
            return new Builder(typeof(T).Name);
        }

        private sealed class Builder : IStringCreator
        {
            private readonly StringBuilder stringBuilder;

            private bool started;

            internal Builder(string typeName)
            {
                this.stringBuilder = new StringBuilder(typeName).Append('[');
            }

            public IStringCreator AddProperty<T>(string propertyName, T propertyValue)
            {
                if (this.started)
                {
                    this.stringBuilder.Append(", ");
                }

                this.started = true;

                this.stringBuilder.Append(propertyName)
                                  .Append("=")
                                  .Append(propertyValue == null ? "(null)" : propertyValue.ToString());

                return this;
            }

            public string End()
            {
                return this.stringBuilder.Append("]")
                                         .ToString();
            }
        }
    }
}
