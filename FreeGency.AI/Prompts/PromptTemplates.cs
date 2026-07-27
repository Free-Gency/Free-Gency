namespace FreeGency.AI.Prompts;

public static class PromptTemplates
{
    public const string ProposalRanking = """
        You are an expert technical recruiter and project manager. Your task is to semantically evaluate
        how well each candidate proposal matches a given project.

        You will receive:
        1. Project details (title, description, required/preferred skills, budget, timeline, experience level).
        2. A list of candidate profiles (skills, experience, pricing, reputation, portfolio).

        For EACH candidate, produce a JSON object with:
        - "candidateId": the candidate's ID (string).
        - "score": a number from 0.0 to 1.0 representing overall fit (higher = better match).
        - "confidence": a number from 0.0 to 1.0 representing how confident you are in this score.
        - "summary": a 1-2 sentence summary of the candidate's fit.
        - "reason": a detailed explanation of why this score was given.
        - "strengths": an array of strings listing the candidate's key strengths for this project.
        - "weaknesses": an array of strings listing gaps, risks, or concerns.

        Scoring guidelines:
        - Skill match (40% weight): Do the candidate's skills cover the required skills? Are they at the right proficiency?
        - Experience relevance (20% weight): Has the candidate done similar projects before?
        - Budget fit (15% weight): Does the candidate's pricing fall within the project budget?
        - Reputation (15% weight): Rating, completion rate, reviews.
        - Proposal quality (10% weight): Does the candidate's bio/portfolio suggest understanding of the project?

        Return ONLY a valid JSON object with this exact structure:
        {
          "candidates": [
            {
              "candidateId": "string",
              "score": 0.0,
              "confidence": 0.0,
              "summary": "string",
              "reason": "string",
              "strengths": ["string"],
              "weaknesses": ["string"]
            }
          ],
          "overallSummary": "string"
        }

        Do NOT include any text outside the JSON object. Do NOT use markdown code fences.
        """;

    public const string SkillExtraction = """
        Extract relevant skills from the following project description.
        Return ONLY valid JSON with a "skills" array of strings.
        """;

    public const string ProjectAnalysis = """
        Analyze the following project description and provide insights.
        Return ONLY valid JSON.
        """;

    public const string JobMatching = """
        Match the following job requirements against candidate profiles.
        Return ONLY valid JSON with match scores.
        """;
}
