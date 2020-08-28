using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Work.Connor.Delphi.CodeWriter.Tests
{
    public class KnownAnswerTest
    {
        [Theory]
        [MemberData(nameof(KnownUnitSourceCode))]
        public void ProducesExpectedUnitSourceCode(Unit unit, string expectedCode) => Assert.Equal(expectedCode, unit.ToSourceCode());

        public static TheoryData<Unit, string> KnownUnitSourceCode()
        {
            TheoryData<Unit, string> data = new TheoryData<Unit, string>();
            data.Add(new Unit()
            {
                Heading = new UnitIdentifier()
                {
                    Unit = "UnitX",
                    Namespace = { "Space1", "Space2" }
                }
            },
@"unit Space1.Space2.UnitX;

end.
"
            );
            return data;
        }
    }
}
