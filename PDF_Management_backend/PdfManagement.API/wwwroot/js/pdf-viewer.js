/**
 * PDF Viewer with Comments
 * Handles displaying and interacting with PDF documents and comments
 */

// Global variables
let currentToken = null;
let currentComments = [];
let currentPage = 1;
let connectorName = null;

// Initialize the viewer
function initViewer(token, connector = null) {
    currentToken = token;
    connectorName = connector;
    
    // Load comments
    loadComments();
    
    // Set up event listeners
    document.getElementById('comment-form').addEventListener('submit', handleCommentSubmit);
    
    // Set up reply functionality
    setupReplyButtons();
}

// Load comments from the server
function loadComments() {
    fetch(`/api/Public/view/${currentToken}`)
        .then(response => response.json())
        .then(data => {
            if (data.success !== false) {
                currentComments = data.comments || [];
                
                // Display connector name if available
                if (connectorName && data.document) {
                    const documentTitle = document.getElementById('document-title');
                    if (documentTitle) {
                        documentTitle.innerHTML = `${data.document.fileName} <span class="connector-name">(via ${connectorName})</span>`;
                    }
                }
                
                renderComments();
            } else {
                showError(data.message || 'Failed to load comments');
            }
        })
        .catch(error => {
            console.error('Error loading comments:', error);
            showError('Failed to load comments. Please try again.');
        });
}

// Render comments in the UI
function renderComments() {
    const commentsContainer = document.getElementById('comments-container');
    commentsContainer.innerHTML = '';
    
    if (currentComments.length === 0) {
        commentsContainer.innerHTML = '<p class="no-comments">No comments yet. Be the first to comment!</p>';
        return;
    }
    
    // Filter comments for current page
    const pageComments = currentComments.filter(comment => comment.pageNumber === currentPage);
    
    pageComments.forEach(comment => {
        // Only render top-level comments here (no parent)
        if (!comment.parentCommentId) {
            const commentElement = createCommentElement(comment);
            commentsContainer.appendChild(commentElement);
        }
    });
    
    // Re-setup reply buttons after rendering
    setupReplyButtons();
}

// Create HTML element for a comment
function createCommentElement(comment) {
    const commentDiv = document.createElement('div');
    commentDiv.className = 'comment';
    commentDiv.dataset.commentId = comment.id;
    
    const header = document.createElement('div');
    header.className = 'comment-header';
    
    const author = document.createElement('span');
    author.className = 'comment-author';
    author.textContent = comment.commenterName || (comment.userType === 'invited' ? 'Guest User' : 'User');
    
    const date = document.createElement('span');
    date.className = 'comment-date';
    date.textContent = new Date(comment.createdAt).toLocaleString();
    
    header.appendChild(author);
    header.appendChild(date);
    
    const content = document.createElement('div');
    content.className = 'comment-content';
    content.textContent = comment.content;
    
    const actions = document.createElement('div');
    actions.className = 'comment-actions';
    
    const replyButton = document.createElement('button');
    replyButton.className = 'reply-button';
    replyButton.textContent = 'Reply';
    replyButton.dataset.commentId = comment.id;
    
    actions.appendChild(replyButton);
    
    commentDiv.appendChild(header);
    commentDiv.appendChild(content);
    commentDiv.appendChild(actions);
    
    // Add reply form container (hidden by default)
    const replyFormContainer = document.createElement('div');
    replyFormContainer.className = 'reply-form-container';
    replyFormContainer.style.display = 'none';
    replyFormContainer.innerHTML = `
        <form class="reply-form" data-parent-id="${comment.id}">
            <textarea required placeholder="Write your reply..."></textarea>
            <div class="form-actions">
                <button type="button" class="cancel-reply">Cancel</button>
                <button type="submit">Submit Reply</button>
            </div>
        </form>
    `;
    commentDiv.appendChild(replyFormContainer);
    
    // Add replies if any
    if (comment.replies && comment.replies.length > 0) {
        const repliesContainer = document.createElement('div');
        repliesContainer.className = 'replies-container';
        
        comment.replies.forEach(reply => {
            const replyElement = createReplyElement(reply);
            repliesContainer.appendChild(replyElement);
        });
        
        commentDiv.appendChild(repliesContainer);
    }
    
    return commentDiv;
}

// Create HTML element for a reply
function createReplyElement(reply) {
    const replyDiv = document.createElement('div');
    replyDiv.className = 'reply';
    replyDiv.dataset.commentId = reply.id;
    
    const header = document.createElement('div');
    header.className = 'reply-header';
    
    const author = document.createElement('span');
    author.className = 'reply-author';
    author.textContent = reply.commenterName || (reply.userType === 'invited' ? 'Guest User' : 'User');
    
    const date = document.createElement('span');
    date.className = 'reply-date';
    date.textContent = new Date(reply.createdAt).toLocaleString();
    
    header.appendChild(author);
    header.appendChild(date);
    
    const content = document.createElement('div');
    content.className = 'reply-content';
    content.textContent = reply.content;
    
    replyDiv.appendChild(header);
    replyDiv.appendChild(content);
    
    return replyDiv;
}

// Set up reply button functionality
function setupReplyButtons() {
    document.querySelectorAll('.reply-button').forEach(button => {
        button.addEventListener('click', function() {
            const commentId = this.dataset.commentId;
            const replyFormContainer = this.closest('.comment').querySelector('.reply-form-container');
            
            // Hide all other reply forms
            document.querySelectorAll('.reply-form-container').forEach(container => {
                if (container !== replyFormContainer) {
                    container.style.display = 'none';
                }
            });
            
            // Toggle this reply form
            replyFormContainer.style.display = replyFormContainer.style.display === 'none' ? 'block' : 'none';
            
            // Set up cancel button
            replyFormContainer.querySelector('.cancel-reply').addEventListener('click', function() {
                replyFormContainer.style.display = 'none';
            });
            
            // Set up form submission
            replyFormContainer.querySelector('form').addEventListener('submit', function(e) {
                e.preventDefault();
                const content = this.querySelector('textarea').value.trim();
                if (content) {
                    submitReply(commentId, content);
                    replyFormContainer.style.display = 'none';
                    this.querySelector('textarea').value = '';
                }
            });
        });
    });
}

// Handle comment form submission
function handleCommentSubmit(e) {
    e.preventDefault();
    
    const form = e.target;
    const content = form.querySelector('textarea').value.trim();
    const commenterName = form.querySelector('#commenter-name').value.trim();
    
    if (!content || !commenterName) {
        showError('Please provide both your name and comment');
        return;
    }
    
    const commentData = {
        content: content,
        pageNumber: currentPage,
        parentCommentId: null,
        commenterName: commenterName
    };
    
    submitComment(commentData);
    form.querySelector('textarea').value = '';
}

// Submit a new comment to the server
function submitComment(commentData) {
    fetch(`/api/Public/comment/${currentToken}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(commentData)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success !== false) {
            // Update the comments with the full list returned from the server
            if (data.allComments) {
                currentComments = data.allComments;
            } else {
                // If server doesn't return all comments, add the new one
                currentComments.push(data.comment || data);
            }
            renderComments();
            showSuccess('Comment added successfully');
        } else {
            showError(data.message || 'Failed to add comment');
        }
    })
    .catch(error => {
        console.error('Error adding comment:', error);
        showError('Failed to add comment. Please try again.');
    });
}

// Submit a reply to the server
function submitReply(parentId, content) {
    // Get the commenter name from the main form to maintain consistency
    const commenterName = document.querySelector('#commenter-name').value.trim();
    
    if (!commenterName) {
        showError('Please provide your name in the main comment form');
        return;
    }
    
    const replyData = {
        content: content,
        pageNumber: currentPage,
        parentCommentId: parseInt(parentId),
        commenterName: commenterName
    };
    
    fetch(`/api/Public/comment/${currentToken}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(replyData)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success !== false) {
            // Update the comments with the full list returned from the server
            if (data.allComments) {
                currentComments = data.allComments;
            } else {
                // If server doesn't return all comments, we need to add the reply to the right parent
                const newReply = data.comment || data;
                
                // Find the parent comment and add the reply
                const parentComment = currentComments.find(c => c.id === parseInt(parentId));
                if (parentComment) {
                    if (!parentComment.replies) {
                        parentComment.replies = [];
                    }
                    parentComment.replies.push(newReply);
                }
            }
            renderComments();
            showSuccess('Reply added successfully');
        } else {
            showError(data.message || 'Failed to add reply');
        }
    })
    .catch(error => {
        console.error('Error adding reply:', error);
        showError('Failed to add reply. Please try again.');
    });
}

// Show success message
function showSuccess(message) {
    const notification = document.createElement('div');
    notification.className = 'notification success';
    notification.textContent = message;
    
    document.body.appendChild(notification);
    
    setTimeout(() => {
        notification.classList.add('show');
    }, 10);
    
    setTimeout(() => {
        notification.classList.remove('show');
        setTimeout(() => {
            document.body.removeChild(notification);
        }, 300);
    }, 3000);
}

// Show error message
function showError(message) {
    const notification = document.createElement('div');
    notification.className = 'notification error';
    notification.textContent = message;
    
    document.body.appendChild(notification);
    
    setTimeout(() => {
        notification.classList.add('show');
    }, 10);
    
    setTimeout(() => {
        notification.classList.remove('show');
        setTimeout(() => {
            document.body.removeChild(notification);
        }, 300);
    }, 3000);
}

// Change page
function changePage(pageNum) {
    currentPage = pageNum;
    renderComments();
}

// Export functions for use in HTML
window.PdfViewer = {
    init: initViewer,
    changePage: changePage,
    setConnectorName: function(name) {
        connectorName = name;
        // Update UI if document title is already displayed
        const documentTitle = document.getElementById('document-title');
        if (documentTitle && documentTitle.textContent.indexOf('(via') === -1) {
            documentTitle.innerHTML = `${documentTitle.textContent} <span class="connector-name">(via ${connectorName})</span>`;
        }
    }
};
