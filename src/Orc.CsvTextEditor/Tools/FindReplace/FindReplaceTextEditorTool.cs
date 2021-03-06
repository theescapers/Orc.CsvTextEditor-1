﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FindReplaceTextEditorTool.cs" company="WildGums">
//   Copyright (c) 2008 - 2017 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.CsvTextEditor
{
    using System.Threading.Tasks;
    using Catel;
    using Catel.IoC;
    using Catel.MVVM;
    using Catel.Services;
    using Catel.Threading;
    using ICSharpCode.AvalonEdit;

    public class FindReplaceTextEditorTool : CsvTextEditorToolBase
    {
        #region Fields
        private readonly IFindReplaceSerivce _findReplaceSerivce;
        private readonly IUIVisualizerService _uiVisualizerService;

        private FindReplaceViewModel _findReplaceViewModel;
        #endregion

        #region Constructors
        public FindReplaceTextEditorTool(TextEditor textEditor, ICsvTextEditorInstance csvTextEditorInstance,
            IUIVisualizerService uiVisualizerService, ITypeFactory typeFactory)
            : base(textEditor, csvTextEditorInstance)
        {
            Argument.IsNotNull(() => uiVisualizerService);
            Argument.IsNotNull(() => typeFactory);

            _uiVisualizerService = uiVisualizerService;

            _findReplaceSerivce = typeFactory.CreateInstanceWithParametersAndAutoCompletion<FindReplaceService>(TextEditor);
        }
        #endregion

        #region Properties
        public override string Name => "CsvTextEditor.FindReplaceTextEditorTool";
        #endregion

        protected override void OnOpen()
        {
            _findReplaceViewModel = new FindReplaceViewModel(CsvTextEditorInstance, _findReplaceSerivce);

            _uiVisualizerService.ShowAsync(_findReplaceViewModel);

            _findReplaceViewModel.ClosedAsync += OnClosedAsync;
        }

        public override void Close()
        {
            base.Close();

            if (_findReplaceViewModel == null)
            {
                return;
            }

            _findReplaceViewModel.ClosedAsync -= OnClosedAsync;

#pragma warning disable 4014
            _findReplaceViewModel.CloseViewModelAsync(null);            
#pragma warning restore 4014
        }
        
        private Task OnClosedAsync(object sender, ViewModelClosedEventArgs args)
        {
            Close();

            return TaskHelper.Completed;
        }
    }
}