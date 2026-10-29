# OpenAI Usage: Cost Estimates and Recommended Limits

This document describes the expected OpenAI API cost for processing meeting and video recordings (transcription followed by a completion/summary), and the restrictions the program should enforce to keep cost predictable.

## Contents

- [Usage profile](#usage-profile)
- [Cost per video](#cost-per-video)
- [Estimated monthly cost](#estimated-monthly-cost)
- [Technical constraints for long recordings](#technical-constraints-for-long-recordings)
- [Recommended restrictions](#recommended-restrictions)
- [Assumptions and sources](#assumptions-and-sources)

## Usage profile

| Use case | Typical length |
|---|---|
| Day-to-day videos | 10 to 15 minutes |
| Large meetings | 30 minutes to 2 hours |

Models used for the estimates below:

| Endpoint | Model |
|---|---|
| Audio transcription (`/v1/audio/transcriptions`) | `gpt-4o-mini-transcribe` |
| Completions (`/v1/chat/completions`) | `gpt-4.1-mini` |

## Cost per video

Costs are in Brazilian reais (R$) at an effective rate of **R$ 5.56 per US$**, which includes IOF (see [Assumptions and sources](#assumptions-and-sources)).

| Video length | Transcription | Completion | Total |
|---|---|---|---|
| 15 min | R$ 0.25 | R$ 0.01 | **R$ 0.26** |
| 30 min | R$ 0.50 | R$ 0.02 | **R$ 0.52** |
| 2 hours | R$ 2.00 | R$ 0.07 | **R$ 2.07** |

Transcription is about 95% of the total cost. Using `whisper-1` instead of `gpt-4o-mini-transcribe` doubles the transcription cost (US$ 0.006 per minute instead of US$ 0.003).

## Estimated monthly cost

A typical month of 20 videos of 15 minutes plus 4 two-hour meetings comes to about **R$ 14**.

## Technical constraints for long recordings

Long recordings are limited more by technical constraints than by cost.

- **File size:** the transcription endpoint accepts files up to 25 MB.
- **Output cap per request:** the `gpt-4o` transcribe models have a small maximum output per request (about 2,000 tokens, per OpenAI's model page), so a 2-hour recording cannot be sent in one request.
- **Context size:** a 2-hour transcript is roughly 24,000 tokens, which `gpt-4.1-mini` handles easily.

## Recommended restrictions

1. **Extract audio first and chunk it.** Do not upload the video itself. Strip the audio with ffmpeg (mono, 16 kHz, low bitrate). Split long recordings into chunks of about 10 to 20 minutes, with a few seconds of overlap, and join the transcripts afterward.
2. **Maximum duration of about 130 minutes.** This covers the longest expected meetings and rejects anything much longer.
3. **Cost check before each job.** Estimate the cost from the audio length before sending, and ask for confirmation if it exceeds a threshold (for example R$ 5). Normal jobs never reach it, so it only triggers on mistakes.
4. **Different output caps by job type.** Use about 1,000 tokens for the 10 to 15 minute videos and 2,000 for the large meetings. Cap the input around 30,000 tokens.
5. **Retry limit.** Allow at most 2 or 3 retries per chunk, not per job. With chunking, a retry loop multiplies quickly.
6. **Daily cap in code and a billing limit on the account.** A daily limit of R$ 20 and a monthly OpenAI budget of R$ 50 to 100 leaves plenty of room above the typical month. Keep auto-recharge off.

### Summary of suggested values

| Setting | Suggested value |
|---|---|
| Audio format sent to the API | Mono, 16 kHz, low bitrate |
| Chunk length | 10 to 20 minutes, with a few seconds of overlap |
| Maximum recording duration | About 130 minutes |
| Maximum file size per request | 25 MB (API limit) |
| Confirmation threshold per job | R$ 5 |
| Completion output cap | About 1,000 tokens (short videos), 2,000 tokens (large meetings) |
| Completion input cap | About 30,000 tokens |
| Retries | 2 to 3 per chunk |
| Daily spend limit (in code) | R$ 20 |
| Monthly budget (OpenAI account) | R$ 50 to 100, auto-recharge off |

## Assumptions and sources

- **Prices (US$):** `gpt-4o-mini-transcribe` US$ 0.003 per minute, `whisper-1` US$ 0.006 per minute, `gpt-4.1-mini` US$ 0.40 per 1M input tokens and US$ 1.60 per 1M output tokens. Source: OpenAI model docs (developers.openai.com). Verify current prices at https://platform.openai.com/pricing.
- **Exchange rate:** PTAX closing rate of R$ 5.3744 per US$ (Banco Central do Brasil, 29 Oct 2026).
- **IOF:** 3.5% on international card purchases (Decreto 12.499/2025), giving an effective rate of R$ 5.5625 per US$. Set to 0% if your card or bank charges no IOF.
- **Token estimates:** about 200 transcript tokens per minute of speech. Completion output of about 800 tokens (15 min), 1,000 tokens (30 min) and 1,500 tokens (2 hours). These are estimates, not measured values.
- **Not included:** cached-input discounts, retries, and any other OpenAI products.
- Prices and exchange rates change, so update the figures before relying on them.


_(AI generated files for costs considerations)_