using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Toreole.Turnbased.Text;

public class TextFormattingTests
{
    // A Test behaves as an ordinary method
    [Test]
    public void TextFormattingTestsSimplePasses()
    {
        object value = 12.55f;
        string format = "0.0";
        string actual = TextFormatting.Format(value, format);
        string expected = 12.55f.ToString(format); // necessary to account for test-host system language formatting specifics (. or ,)
        Assert.That(actual, Is.EqualTo(expected));

        value = "12.55";
        actual = TextFormatting.Format(value, "0.0");
        Assert.That(actual, Is.EqualTo(value));
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator TextFormattingTestsWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
