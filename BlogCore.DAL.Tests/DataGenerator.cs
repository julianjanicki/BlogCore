namespace BlogCore.DAL.Tests;

using BlogCore.DAL.Models;
using Bogus;

public static class DataGenerator
{
    public static Faker<Post> GetPostFaker() => new Faker<Post>()
        .RuleFor(p => p.Id, f => 0) // EF wygeneruje Id przy zapisie
        .RuleFor(p => p.Author, f => f.Name.FullName())
        .RuleFor(p => p.Content, f => f.Lorem.Paragraph());

    public static Faker<Comment> GetCommentFaker(int postId) => new Faker<Comment>()
        .RuleFor(c => c.Id, f => 0)
        .RuleFor(c => c.PostId, _ => postId)
        .RuleFor(c => c.Content, f => f.Lorem.Sentence());
}
