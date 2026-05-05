namespace BlogCore.DAL.Tests;

using BlogCore.DAL.Models;
using Microsoft.EntityFrameworkCore;

[TestClass]
public sealed class BlogRepositoryTests : IntegrationTestBase
{
    [TestMethod]
    public void AddPost_WhenAddingNewPost_IncreasesCountByOne()
    {
        var countBefore = _repository.GetAllPosts().Count();
        var post = DataGenerator.GetPostFaker().Generate();
        _repository.AddPost(post);
        var countAfter = _repository.GetAllPosts().Count();
        Assert.AreEqual(countBefore + 1, countAfter);
    }

    [TestMethod]
    [ExpectedException(typeof(DbUpdateException))]
    public void AddPost_NullContent_ThrowsDbUpdateException()
    {
        var invalidPost = new Post
        {
            Author = "Jan Kowalski",
            Content = null!
        };
        _repository.AddPost(invalidPost);
    }

    [TestMethod]
    public async Task GetCommentsByPostId_WhenPostHasThreeComments_ReturnsExactlyThree()
    {
        var post = DataGenerator.GetPostFaker().Generate();
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var comments = DataGenerator.GetCommentFaker(post.Id).Generate(3);
        _context.Comments.AddRange(comments);
        await _context.SaveChangesAsync();

        var result = _repository.GetCommentsByPostId(post.Id);
        Assert.AreEqual(3, result.Count());
    }

    [TestMethod]
    public void GetAllPosts_EmptyDb_ReturnsZero()
    {
        var result = _repository.GetAllPosts();
        Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public void AddPost_LongContent_SavesCorrectly()
    {
        var faker = DataGenerator.GetPostFaker();
        var post = new Post
        {
            Author = faker.Generate().Author,
            Content = string.Join("\n\n", new Bogus.Faker().Lorem.Paragraphs(5))
        };
        _repository.AddPost(post);
        var saved = _repository.GetAllPosts().First(p => p.Author == post.Author);
        Assert.AreEqual(post.Content, saved.Content);
    }

    [TestMethod]
    public void AddPost_SpecialCharactersInAuthor_SavesCorrectly()
    {
        var post = new Post
        {
            Author = "Zażółć Gęślą Jaźń 123!",
            Content = "Treść posta."
        };
        _repository.AddPost(post);
        var saved = _repository.GetAllPosts().First(p => p.Author == post.Author);
        Assert.AreEqual("Zażółć Gęślą Jaźń 123!", saved.Author);
    }

    [TestMethod]
    public async Task AddComment_ValidData_IncreasesCountForPost()
    {
        var post = DataGenerator.GetPostFaker().Generate();
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var comment = DataGenerator.GetCommentFaker(post.Id).Generate();
        _repository.AddComment(comment);

        var comments = _repository.GetCommentsByPostId(post.Id);
        Assert.AreEqual(1, comments.Count());
    }

    [TestMethod]
    public void GetCommentsByPostId_NonExistentPost_ReturnsEmpty()
    {
        var result = _repository.GetCommentsByPostId(99999);
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    [ExpectedException(typeof(DbUpdateException))]
    public void AddComment_OrphanComment_ThrowsException()
    {
        var comment = DataGenerator.GetCommentFaker(99999).Generate();
        _repository.AddComment(comment);
    }

    [TestMethod]
    public async Task MultipleComments_DifferentPosts_ReturnsOnlyCorrectOnes()
    {
        var post1 = DataGenerator.GetPostFaker().Generate();
        var post2 = DataGenerator.GetPostFaker().Generate();
        _context.Posts.AddRange(post1, post2);
        await _context.SaveChangesAsync();

        var comments1 = DataGenerator.GetCommentFaker(post1.Id).Generate(5);
        var comments2 = DataGenerator.GetCommentFaker(post2.Id).Generate(2);
        _context.Comments.AddRange(comments1);
        _context.Comments.AddRange(comments2);
        await _context.SaveChangesAsync();

        var forPost1 = _repository.GetCommentsByPostId(post1.Id);
        var forPost2 = _repository.GetCommentsByPostId(post2.Id);

        Assert.AreEqual(5, forPost1.Count());
        Assert.AreEqual(2, forPost2.Count());
    }

    [TestMethod]
    [ExpectedException(typeof(DbUpdateException))]
    public void AddPost_NullAuthor_ThrowsDbUpdateException()
    {
        var invalidPost = new Post
        {
            Author = null!,
            Content = "Treść"
        };
        _repository.AddPost(invalidPost);
    }

    [TestMethod]
    [ExpectedException(typeof(DbUpdateException))]
    public async Task AddComment_NullContent_ThrowsDbUpdateException()
    {
        var post = DataGenerator.GetPostFaker().Generate();
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var invalidComment = new Comment
        {
            PostId = post.Id,
            Content = null!
        };
        _repository.AddComment(invalidComment);
    }

    [TestMethod]
    public async Task DeletePost_CascadeDeleteComments()
    {
        var post = DataGenerator.GetPostFaker().Generate();
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var comments = DataGenerator.GetCommentFaker(post.Id).Generate(3);
        _context.Comments.AddRange(comments);
        await _context.SaveChangesAsync();

        Assert.AreEqual(3, _repository.GetCommentsByPostId(post.Id).Count());

        _repository.DeletePost(post);

        Assert.AreEqual(0, _repository.GetCommentsByPostId(post.Id).Count());
    }
}
