using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXChineseCompletion
{

    [Export(typeof(IAsyncCompletionSourceProvider))]
    [Name("ChineseCompletion")]
    [ContentType("CSharp")]
    [Order(After = "ChineseCompletionSourceProvider")]
    internal class ChineseCompletionSourceProvider : IAsyncCompletionSourceProvider
    {

        private IDictionary<ITextView, IAsyncCompletionSource> cache = new Dictionary<ITextView, IAsyncCompletionSource>();

        [Import]
        private IAsyncCompletionBroker broker;

        [Import]
        private ITextStructureNavigatorSelectorService structureNavigatorSelector;

        public IAsyncCompletionSource GetOrCreate(ITextView textView)
        {
            if (cache.TryGetValue(textView, out var itemSource)) return itemSource;

            IAsyncCompletionSource sharpSource = GetSharpSource(textView);
            var source = new ChineseCompletionSource(structureNavigatorSelector, sharpSource);
            textView.Closed += (o, e) => cache.Remove(textView);
            cache.Add(textView, source);
            return source;
        }

        private IAsyncCompletionSource GetSharpSource(ITextView textView)
        {
            var orderedCompletionSourceProviders = broker.GetType().GetField("orderedCompletionSourceProviders", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (orderedCompletionSourceProviders != null)
            {
                IList list = orderedCompletionSourceProviders.GetValue(broker) as IList;
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        Lazy<IAsyncCompletionSourceProvider> provider = list[i] as Lazy<IAsyncCompletionSourceProvider>;
                        if (provider.IsValueCreated)
                        {
                            Type type = provider.Value.GetType();
                            if (type.FullName == "Microsoft.CodeAnalysis.Editor.Implementation.IntelliSense.AsyncCompletion.CompletionSourceProvider")
                            {
                                return provider.Value.GetOrCreate(textView);
                            }
                        }
                    }
                }
            }
            return null;
        }

    }
}
