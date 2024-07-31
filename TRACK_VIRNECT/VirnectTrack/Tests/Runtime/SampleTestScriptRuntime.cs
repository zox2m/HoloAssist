using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests {
/// <summary>
/// Initial test script for runtime tests: Both tests will always pass
/// This is for testing the automated test framework
/// </summary>
public class SampleTestScriptRuntime
{
    // A Test behaves as an ordinary method
    [Test]
    public void NewTestScriptSimplePasses()
    {
        // Use the Assert class to test conditions
        // Assert.True(false);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator NewTestScriptWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
}