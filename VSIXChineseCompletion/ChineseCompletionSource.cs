using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Operations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace VSIXChineseCompletion
{
    class ChineseCompletionSource : IAsyncCompletionSource
    {

        private ITextStructureNavigatorSelectorService StructureNavigatorSelector { get; }

        private readonly IAsyncCompletionSource sharpSource;

        private static ImageElement PinYinIcon = new ImageElement(new ImageId(new Guid("1d6c1255-d8dc-496f-bd85-5f6b1ec02eea"), 1), "PinYin");
        private static CompletionFilter PinYinFilter = new CompletionFilter("PinYin", "P", PinYinIcon);
        private static ImmutableArray<CompletionFilter> PinYinFilters = ImmutableArray.Create(PinYinFilter);

        public ChineseCompletionSource(ITextStructureNavigatorSelectorService structureNavigatorSelector, IAsyncCompletionSource sharpSource)
        {
            StructureNavigatorSelector = structureNavigatorSelector;
            this.sharpSource = sharpSource;
        }

        public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
        {
            var tokenSpan = FindTokenSpanAtPosition(triggerLocation);
            return new CompletionStartData(CompletionParticipation.ProvidesItems, tokenSpan);
        }

        private SnapshotSpan FindTokenSpanAtPosition(SnapshotPoint triggerLocation)
        {
            ITextStructureNavigator navigator = StructureNavigatorSelector.GetTextStructureNavigator(triggerLocation.Snapshot.TextBuffer);
            TextExtent extent = navigator.GetExtentOfWord(triggerLocation);
            if (triggerLocation.Position > 0 && (!extent.IsSignificant || !extent.Span.GetText().Any(c => char.IsLetterOrDigit(c))))
            {
                extent = navigator.GetExtentOfWord(triggerLocation - 1);
            }
            var tokenSpan = triggerLocation.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
            var snapshot = triggerLocation.Snapshot;
            var tokenText = tokenSpan.GetText(snapshot);
            if (string.IsNullOrWhiteSpace(tokenText))
            {
                return new SnapshotSpan(triggerLocation, 0);
            }
            int startOffset = 0;
            int endOffset = 0;
            if (tokenText.Length > 0)
            {
                if (tokenText.StartsWith("\""))
                    startOffset = 1;
            }
            if (tokenText.Length - startOffset > 0)
            {
                if (tokenText.EndsWith("\"\r\n"))
                    endOffset = 3;
                else if (tokenText.EndsWith("\r\n"))
                    endOffset = 2;
                else if (tokenText.EndsWith("\"\n"))
                    endOffset = 2;
                else if (tokenText.EndsWith("\n"))
                    endOffset = 1;
                else if (tokenText.EndsWith("\""))
                    endOffset = 1;
            }
            return new SnapshotSpan(tokenSpan.GetStartPoint(snapshot) + startOffset, tokenSpan.GetEndPoint(snapshot) - endOffset);
        }

        public async Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            List<CompletionItem> items = new List<CompletionItem>();
            if (sharpSource != null)
            {
                CompletionContext context = await sharpSource.GetCompletionContextAsync(session, trigger, triggerLocation, applicableToSpan, token);

                for (int i = 0; i < context.Items.Length; i++)
                {
                    string rawStr = context.Items[i].InsertText;

                    if (rawStr.HasChinese())
                    {
                        List<string> pinyinStr = PinYinConverterHelp.GetTotalPingYin(rawStr, out var hasChinese, true);
                        if (hasChinese)
                        {

                            for (int t = 0; t < pinyinStr.Count; t++)
                            {
                                string pinyin = pinyinStr[t];

                                ImageElement icon = PinYinIcon;
                                ImmutableArray<CompletionFilter> filters = PinYinFilters;
                                var item = new CompletionItem(
                                    displayText: $"{rawStr}",
                                    source: this,
                                    icon: icon,
                                    filters: filters,
                                    suffix: pinyin,
                                    insertText: pinyin,
                                    sortText: pinyin,
                                    filterText: pinyin,
                                    attributeIcons: ImmutableArray<ImageElement>.Empty);
                                item.Properties.AddProperty("PinYin", $"< {pinyin} ({rawStr}) >");

                                items.Add(item);
                            }
                        }
                    }
                }
            }
            return new CompletionContext(items.ToImmutableArray());
        }

        public async Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token)
        {
            if (item.Properties.TryGetProperty<string>("PinYin", out var matchingElement))
            {
                return matchingElement;
            }
            return null;
        }

    }
}
