import { http, HttpResponse } from 'msw';
import {
  mentorQuestionnaireFixture,
  questionnaireFixture,
  profiles,
} from '../fixtures/api';

type MockApplication = {
  id: string;
  mentorId: string;
  mentorApproved: boolean | null;
  tutorApproved: boolean | null;
  reasonForRefusal: string | null;
};
let applications: MockApplication[] = [];
const persistent = import.meta.env.DEV && !import.meta.env.TEST;
function read(): MockApplication[] {
  return persistent
    ? (JSON.parse(
        sessionStorage.getItem('kurator.mock.applications') ?? '[]',
      ) as MockApplication[])
    : applications;
}
function save(items: MockApplication[]) {
  applications = items;
  if (persistent)
    sessionStorage.setItem('kurator.mock.applications', JSON.stringify(items));
}
export function resetApplications() {
  save([]);
}
export function seedApplication(overrides: Partial<MockApplication> = {}) {
  const item: MockApplication = {
    id: crypto.randomUUID(),
    mentorId: profiles[1]!.id,
    mentorApproved: null,
    tutorApproved: null,
    reasonForRefusal: null,
    ...overrides,
  };
  save([...read(), item]);
  return item;
}
function result(items: unknown[], tutor = false) {
  return items.length
    ? HttpResponse.json(tutor ? { listForTutorApp: items } : { listApp: items })
    : new HttpResponse(null, { status: 404 });
}

export const applicationHandlers = [
  http.post('*/api/applications', async ({ request }) => {
    const mentorId = (await request.json()) as string;
    const item = seedApplication({ mentorId });
    return HttpResponse.json(
      { appDetail: { id: item.id, studentId: profiles[0]!.id, mentorId } },
      { status: 201 },
    );
  }),
  http.get('*/api/applications', () =>
    result(
      read().map((item) => ({
        id: item.id,
        questionnaireForm: mentorQuestionnaireFixture,
        reasonForRefusal: item.reasonForRefusal,
      })),
    ),
  ),
  http.delete('*/api/applications', ({ request }) => {
    const id = new URL(request.url).searchParams.get('id');
    save(read().filter((item) => item.id !== id));
    return new HttpResponse(null, { status: 204 });
  }),
  http.get('*/api/applications/mentor/get', () =>
    result(
      read()
        .filter((item) => item.mentorApproved === null)
        .map((item) => ({
          id: item.id,
          questionnaireForm: questionnaireFixture,
          reasonForRefusal: null,
        })),
    ),
  ),
  http.get('*/api/applications/tutor/get', () =>
    result(
      read()
        .filter(
          (item) => item.mentorApproved === true && item.tutorApproved === null,
        )
        .map((item) => ({
          id: item.id,
          studentQuestionnaireForm: questionnaireFixture,
          mentorQuestionnaireForm: mentorQuestionnaireFixture,
        })),
      true,
    ),
  ),
  http.put(
    '*/api/applications/:role/:decision',
    async ({ request, params }) => {
      const decline = params.decision === 'refusal';
      const body = decline
        ? ((await request.json()) as { id: string; reasonForRefusal: string })
        : null;
      const id = body?.id ?? new URL(request.url).searchParams.get('id');
      save(
        read().map((item) =>
          item.id !== id
            ? item
            : {
                ...item,
                ...(params.role === 'mentor'
                  ? { mentorApproved: !decline }
                  : { tutorApproved: !decline }),
                reasonForRefusal: body?.reasonForRefusal ?? null,
              },
        ),
      );
      return HttpResponse.json({ message: 'Approved', status: 200 });
    },
  ),
];
