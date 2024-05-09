using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public enum BlogReaction
    {
        UpVote = 0,
        DownVote = 1
    }

    public class Reactions
    {
        [Key]
        public int ReactionId { get; set; }
        public BlogReaction type { get; set; }


    }
}
