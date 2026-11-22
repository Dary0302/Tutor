export const profiles = [
  {
    id: '11111111-1111-4111-8111-111111111111',
    email: 'menti@example.test',
    role: 0,
  },
  {
    id: '22222222-2222-4222-8222-222222222222',
    email: 'mentor@example.test',
    role: 1,
  },
];
export const refreshTokens = [
  'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa',
  'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb',
];
export function mockSession(role = 0) {
  return {
    ...profiles[role],
    token: `test-token-${role}`,
    refreshToken: refreshTokens[role],
  };
}
export const questionnaireFixture = {
  firstName: 'Анна',
  lastName: 'Иванова',
  city: 'Москва, UTC+3',
  education: 'МГУ',
  goalDescription: 'Изучить frontend',
  communicationFormat: 'Online',
  monthsNumberMin: 3,
  monthsNumberMax: 6,
  hoursNumberWeek: 2,
  skills: [{ name: 'TypeScript', skillLevel: 'Beginner' }],
  availability: [
    { dayOfWeek: 'Monday', startTime: '18:00:00', endTime: '19:00:00' },
  ],
  teacherInfo: null,
};

export const mentorFixture = {
  userId: profiles[1]!.id,
  questionnaireId: '33333333-3333-4333-8333-333333333333',
  firstName: 'Мария',
  lastName: 'Петрова',
  position: 'Наставник по разработке',
  placeOfWork: 'Тестовая компания',
  yearsExperience: 5,
  skills: [{ id: '44444444-4444-4444-8444-444444444444', name: 'TypeScript' }],
  rating: 4.7,
  hoursPerWeek: 2.5,
  availablePlaces: 2,
  compatibility: 86,
};
export const mentorQuestionnaireFixture = {
  ...questionnaireFixture,
  firstName: mentorFixture.firstName,
  lastName: mentorFixture.lastName,
  teacherInfo: {
    placeOfWork: mentorFixture.placeOfWork,
    position: mentorFixture.position,
    yearsExperience: mentorFixture.yearsExperience,
    maxStudents: 3,
  },
};
