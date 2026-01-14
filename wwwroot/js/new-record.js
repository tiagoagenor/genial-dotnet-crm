// New Record Page JavaScript
(function() {
    'use strict';

    // Wait for DOM to be ready
    document.addEventListener('DOMContentLoaded', function() {
        // Check if required variables are defined
        if (typeof window.newRecordConfig === 'undefined') {
            console.error('newRecordConfig is not defined. Make sure to include the inline script before this file.');
            return;
        }

        const { stages, currentStage } = window.newRecordConfig;

        // Update stage display
        function updateStageDisplay(stage) {
            const stageData = stages[stage];
            if (stageData) {
                const letterEl = document.getElementById('stageLetter');
                const tooltipEl = document.getElementById('stageTooltip');
                if (letterEl) letterEl.textContent = stageData.letter;
                if (tooltipEl) tooltipEl.textContent = stageData.name;
            }
        }

        // Initialize stage display
        updateStageDisplay(currentStage);

        // Open modal
        const stageSelector = document.getElementById('stageSelector');
        if (stageSelector) {
            stageSelector.addEventListener('click', function() {
                const stageModal = document.getElementById('stageModal');
                if (stageModal) {
                    stageModal.classList.remove('hidden');
                }
            });
        }

        // Close modal
        const closeModal = document.getElementById('closeModal');
        if (closeModal) {
            closeModal.addEventListener('click', function() {
                const stageModal = document.getElementById('stageModal');
                if (stageModal) {
                    stageModal.classList.add('hidden');
                }
            });
        }

        // Close modal when clicking outside
        const stageModal = document.getElementById('stageModal');
        if (stageModal) {
            stageModal.addEventListener('click', function(e) {
                if (e.target === this) {
                    this.classList.add('hidden');
                }
            });
        }

        // Store Summernote editor instances
        const summernoteEditors = {};

        // Initialize Summernote editors for rich-editor fields
        function initSummernoteEditors() {
            if (typeof $ === 'undefined' || typeof $(document).summernote === 'undefined') {
                setTimeout(initSummernoteEditors, 100);
                return;
            }

            // Find all rich-editor textareas
            const richEditorFields = document.querySelectorAll('textarea.rich-editor-field');
            richEditorFields.forEach(textarea => {
                const fieldName = textarea.name;
                if (!summernoteEditors[fieldName]) {
                    $(`#${textarea.id}`).summernote({
                        height: 400,
                        toolbar: [
                            ['style', ['style']],
                            ['font', ['bold', 'italic', 'underline', 'clear']],
                            ['fontname', ['fontname']],
                            ['color', ['color']],
                            ['para', ['ul', 'ol', 'paragraph']],
                            ['table', ['table']],
                            ['insert', ['link', 'picture', 'video']],
                            ['view', ['fullscreen', 'codeview', 'help']]
                        ],
                        callbacks: {
                            onChange: function(contents, $editable) {
                                // Update textarea value when content changes
                                textarea.value = contents;
                            }
                        }
                    });
                    
                    // Store editor instance (jQuery object)
                    summernoteEditors[fieldName] = $(`#${textarea.id}`);
                }
            });
        }

        // Initialize Summernote editors
        initSummernoteEditors();

        // Handle form submit - update Summernote content before submit
        const form = document.getElementById('newRecordForm');
        if (form) {
            form.addEventListener('submit', function(e) {
                // Get Summernote editor content for rich-editor fields and update textareas
                Object.keys(summernoteEditors).forEach(fieldName => {
                    const editor = summernoteEditors[fieldName];
                    if (editor && typeof $ !== 'undefined') {
                        // Get HTML content from Summernote and update the textarea
                        const html = editor.summernote('code');
                        const textarea = document.getElementById(`record_field_${fieldName}`);
                        if (textarea) {
                            textarea.value = html;
                        }
                    }
                });
                // Let the form submit normally
            });
        }
    });
})();
