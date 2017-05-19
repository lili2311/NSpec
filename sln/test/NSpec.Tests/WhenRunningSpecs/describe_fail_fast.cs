﻿using System.Linq;
using NUnit.Framework;
using FluentAssertions;

namespace NSpec.Tests.WhenRunningSpecs
{
    [TestFixture]
    public class describe_fail_fast : when_running_specs
    {
        class SpecClass : nspec
        {
            void given_a_spec_with_multiple_failures()
            {
                it["this one isn't a failure"] = () => Assert.That("not failure", Is.EqualTo("not failure"));

                it["this one is a failure"] = () => Assert.That("hi", Is.EqualTo("hello"));

                it["this one also fails"] = () => Assert.That("another", Is.EqualTo("failure"));

                context["nested examples"] = () =>
                {
                    it["is skipped"] = () => Assert.That("skipped", Is.EqualTo("skipped"));

                    it["is also skipped"] = () => Assert.That("skipped", Is.EqualTo("skipped"));
                };
            }

            void a_context_that_should_not_run()
            {
                it["does not run because of failure on line 20"] = () => Assert.That(true, Is.True);

                it["also does not run because of failure on line 20"] = () => Assert.That(true, Is.True);
            }
        }

        [SetUp]
        public void Setup()
        {
            failFast = true;
            Run(typeof(SpecClass));
        }

        [Test]
        public void should_skip()
        {
            TheExample("is skipped").HasRun.Should().BeFalse();
        }

        [Test]
        public void only_two_examples_are_executed_one_will_be_a_failure()
        {
            var expecteds = new[]
            {
                "this one isn't a failure",
                "this one is a failure",
            };

            var actuals = AllExamples().Where(e => e.HasRun).Select(e => e.Spec);

            actuals.ShouldBeEquivalentTo(expecteds);
        }

        [Test]
        public void only_executed_contexts_are_printed()
        {
            var expecteds = new[]
            {
                "SpecClass",
                "given a spec with multiple failures",
            };

            var actuals = formatter.WrittenContexts.Select(c => c.Name);

            actuals.ShouldBeEquivalentTo(expecteds);
        }

        [Test]
        public void only_executed_examples_are_printed()
        {
            var expecteds = new[]
            {
                "this one isn't a failure",
                "this one is a failure",
            };

            var actuals = formatter.WrittenExamples.Select(e => e.Spec);

            actuals.ShouldBeEquivalentTo(expecteds);
        }
    }
}
